using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Arbitro.Partidos
{
    public class ReporteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public ReporteModel(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [BindProperty(SupportsGet = true)]
        public int PartidoId { get; set; }

        public ReportePartido? Reporte { get; set; }
        public Partido? Partido { get; set; }
        public List<IncidenciaPartido> Incidencias { get; set; } = new();
        public int ArbitroId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public ReporteInputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            return await CargarAsync();
        }

        private async Task<IActionResult> CargarAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Arbitro)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Arbitro == null) return RedirectToPage("/Error");
            ArbitroId = persona.Arbitro.Id;

            Partido = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Arbitro).ThenInclude(a => a!.Persona)
                .FirstOrDefaultAsync(p => p.Id == PartidoId);

            if (Partido == null) return NotFound();
            if (Partido.ArbitroId != ArbitroId) return RedirectToPage("/Arbitro/Partidos/Index");
            if (Partido.Estado != EstadosPartido.Finalizado)
            {
                ErrorMessage = "El reporte solo está disponible para partidos finalizados.";
                return Page();
            }

            // Load or create Reporte
            Reporte = await _context.ReportesPartido
                .FirstOrDefaultAsync(r => r.PartidoId == PartidoId);

            if (Reporte == null)
            {
                Reporte = new ReportePartido
                {
                    PartidoId = PartidoId,
                    ArbitroId = ArbitroId,
                    Estado = EstadosReporte.Borrador,
                    MinutosJugados = 90,
                    FechaCreacion = DateTime.UtcNow
                };
                _context.ReportesPartido.Add(Reporte);
                await _context.SaveChangesAsync();
            }

            Incidencias = await _context.IncidenciasPartido
                .Include(i => i.Equipo)
                .Include(i => i.EjtJugador).ThenInclude(ejt => ejt!.Jugador).ThenInclude(j => j.Persona)
                .Include(i => i.EjtJugadorSalida).ThenInclude(ejt => ejt!.Jugador).ThenInclude(j => j.Persona)
                .Where(i => i.PartidoId == PartidoId)
                .OrderBy(i => i.Minuto)
                .ThenBy(i => i.FechaCreacion)
                .ToListAsync();

            Input = new ReporteInputModel
            {
                Observaciones = Reporte.Observaciones,
                MinutosJugados = Reporte.MinutosJugados ?? 90,
                HuboProrroga = Reporte.HuboProrroga,
                HuboPenales = Reporte.HuboPenales,
                GolesLocalPenales = Reporte.GolesLocalPenales,
                GolesVisitantePenales = Reporte.GolesVisitantePenales
            };

            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            await GuardarReporteAsync(EstadosReporte.Borrador);
            SuccessMessage = "Borrador guardado correctamente.";
            return await CargarAsync();
        }

        public async Task<IActionResult> OnPostEnviarAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            await GuardarReporteAsync(EstadosReporte.Enviado);
            SuccessMessage = "Reporte enviado correctamente.";
            return await CargarAsync();
        }

        public async Task<IActionResult> OnGetPdfAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            var reporte = await _context.ReportesPartido
                .Include(r => r.Partido).ThenInclude(p => p.EquipoLocal)
                .Include(r => r.Partido).ThenInclude(p => p.EquipoVisitante)
                .Include(r => r.Partido).ThenInclude(p => p.Torneo)
                .Include(r => r.Arbitro).ThenInclude(a => a.Persona)
                .Include(r => r.Incidencias).ThenInclude(i => i.Equipo)
                .Include(r => r.Incidencias).ThenInclude(i => i.EjtJugador)
                    .ThenInclude(ejt => ejt!.Jugador).ThenInclude(j => j.Persona)
                .FirstOrDefaultAsync(r => r.PartidoId == PartidoId);

            if (reporte == null) return NotFound();

            byte[] pdf = _pdfService.GenerarReportePartidoPdf(reporte);
            return File(pdf, "application/pdf", $"Reporte_Partido_{PartidoId}.pdf");
        }

        private async Task GuardarReporteAsync(string estado)
        {
            var reporte = await _context.ReportesPartido.FirstOrDefaultAsync(r => r.PartidoId == PartidoId);
            if (reporte == null) return;

            if (reporte.Estado == EstadosReporte.Enviado && estado == EstadosReporte.Borrador) return;

            reporte.Observaciones = Input.Observaciones;
            reporte.MinutosJugados = Input.MinutosJugados;
            reporte.HuboProrroga = Input.HuboProrroga;
            reporte.HuboPenales = Input.HuboPenales;
            reporte.GolesLocalPenales = Input.HuboPenales ? Input.GolesLocalPenales : null;
            reporte.GolesVisitantePenales = Input.HuboPenales ? Input.GolesVisitantePenales : null;
            reporte.Estado = estado;
            reporte.FechaActualizacion = DateTime.UtcNow;

            if (estado == EstadosReporte.Enviado && !reporte.FechaEnvio.HasValue)
                reporte.FechaEnvio = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    public class ReporteInputModel
    {
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Minutos Jugados")]
        [Range(1, 180)]
        public int MinutosJugados { get; set; } = 90;

        [Display(Name = "¿Hubo Prórroga?")]
        public bool HuboProrroga { get; set; }

        [Display(Name = "¿Hubo Penales?")]
        public bool HuboPenales { get; set; }

        [Display(Name = "Goles Penales (Local)")]
        [Range(0, 20)]
        public int? GolesLocalPenales { get; set; }

        [Display(Name = "Goles Penales (Visitante)")]
        [Range(0, 20)]
        public int? GolesVisitantePenales { get; set; }
    }
}
