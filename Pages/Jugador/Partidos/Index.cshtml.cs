using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Jugador.Partidos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConvocatoriaService _convocatoriaService;

        public IndexModel(ApplicationDbContext context, IConvocatoriaService convocatoriaService)
        {
            _context = context;
            _convocatoriaService = convocatoriaService;
        }

        public string NombreCompleto { get; set; } = string.Empty;
        public string? NombreEquipo { get; set; }
        public List<PartidoJugadorViewModel> PartidosEnCurso { get; set; } = new();
        public List<PartidoJugadorViewModel> ProximosPartidos { get; set; } = new();
        public List<PartidoJugadorViewModel> PartidosPasados { get; set; } = new();
        public string? SuccessMessage { get; set; }

        private int? _ejtId;
        private List<EquipoJugadorTorneo> _inscripciones = new();

        public async Task<IActionResult> OnGetAsync(string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message)) SuccessMessage = message;

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Jugador == null) return RedirectToPage("/Error");

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";
            var jugadorId = persona.Jugador.Id;

            // Teams where this jugador is active
            _inscripciones = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Equipo)
                .Where(ejt => ejt.JugadorId == jugadorId && ejt.Activo)
                .ToListAsync();

            var equipoIds = _inscripciones.Select(e => e.EquipoId).Distinct().ToList();

            // Use the most recent team name
            NombreEquipo = _inscripciones
                .OrderByDescending(e => e.FechaInscripcion)
                .FirstOrDefault()?.Equipo?.Nombre;

            if (!equipoIds.Any()) return Page();

            var partidos = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Arbitro).ThenInclude(a => a!.Persona)
                .Where(p => (equipoIds.Contains(p.EquipoLocalId) || equipoIds.Contains(p.EquipoVisitanteId))
                         && p.Estado != EstadosPartido.Cancelado)
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            // Fetch convocatorias for relevant partidos
            var partidoIds = partidos.Select(p => p.Id).ToList();
            var convocatorias = await _context.Convocatorias
                .Include(c => c.ConvocatoriaJugadores)
                .Where(c => partidoIds.Contains(c.PartidoId) && equipoIds.Contains(c.EquipoId))
                .ToListAsync();

            var mapped = partidos.Select(p =>
            {
                var miEquipoId = equipoIds.Contains(p.EquipoLocalId) ? p.EquipoLocalId : p.EquipoVisitanteId;
                var miInscripcion = _inscripciones.FirstOrDefault(i => i.EquipoId == miEquipoId);

                // Find convocatoria for my team in this partido
                var convocatoria = convocatorias.FirstOrDefault(c => c.PartidoId == p.Id && c.EquipoId == miEquipoId);

                string estadoConvocatoria = "SinConvocatoria";
                int convocatoriaId = 0;
                int ejtId = miInscripcion?.Id ?? 0;
                string estadoConfirmacion = "Pendiente";

                if (convocatoria != null && convocatoria.Estado == EstadosConvocatoria.Publicada)
                {
                    convocatoriaId = convocatoria.Id;
                    var cj = convocatoria.ConvocatoriaJugadores
                        .FirstOrDefault(c => c.EquipoJugadorTorneoId == ejtId);
                    if (cj != null)
                    {
                        estadoConvocatoria = cj.Rol; // Titular/Suplente/Reserva
                        estadoConfirmacion = cj.EstadoConfirmacion;
                    }
                    else
                    {
                        estadoConvocatoria = "NoConvocado";
                    }
                }
                else if (convocatoria != null && convocatoria.Estado == EstadosConvocatoria.Borrador)
                {
                    estadoConvocatoria = "Pendiente";
                }

                return new PartidoJugadorViewModel
                {
                    Id = p.Id,
                    NombreTorneo = p.Torneo.Nombre,
                    NombreLocal = p.EquipoLocal.Nombre,
                    AbrevLocal = p.EquipoLocal.Abreviacion,
                    ColorLocal = p.EquipoLocal.ColorPrimario,
                    LogoLocal = p.EquipoLocal.Logo,
                    NombreVisitante = p.EquipoVisitante.Nombre,
                    AbrevVisitante = p.EquipoVisitante.Abreviacion,
                    ColorVisitante = p.EquipoVisitante.ColorPrimario,
                    LogoVisitante = p.EquipoVisitante.Logo,
                    Fecha = p.Fecha,
                    Sede = p.Sede,
                    Estado = p.Estado,
                    GolesLocal = p.GolesLocal,
                    GolesVisitante = p.GolesVisitante,
                    NombreArbitro = p.Arbitro != null
                        ? $"{p.Arbitro.Persona.Nombre} {p.Arbitro.Persona.ApellidoPaterno}" : null,
                    EsMiEquipoLocal = equipoIds.Contains(p.EquipoLocalId),
                    EstadoConvocatoria = estadoConvocatoria,
                    ConvocatoriaId = convocatoriaId,
                    EjtId = ejtId,
                    EstadoConfirmacion = estadoConfirmacion
                };
            }).ToList();

            PartidosEnCurso = mapped.Where(p => p.Estado == EstadosPartido.EnCurso).ToList();
            ProximosPartidos = mapped.Where(p => p.Estado == EstadosPartido.Programado || p.Estado == EstadosPartido.Reprogramado).ToList();
            PartidosPasados = mapped.Where(p => p.Estado == EstadosPartido.Finalizado)
                                    .OrderByDescending(p => p.Fecha).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmarAsync(int ejtId, int convocatoriaId, string estado)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            await _convocatoriaService.ConfirmarAsistenciaAsync(convocatoriaId, ejtId, estado, null);
            string msg = estado == EstadosConfirmacion.Confirmo ? "Asistencia confirmada." : "Marcado como no disponible.";
            return RedirectToPage(new { message = msg });
        }
    }

    public class PartidoJugadorViewModel
    {
        public int Id { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string NombreLocal { get; set; } = string.Empty;
        public string? AbrevLocal { get; set; }
        public string? ColorLocal { get; set; }
        public string? LogoLocal { get; set; }
        public string NombreVisitante { get; set; } = string.Empty;
        public string? AbrevVisitante { get; set; }
        public string? ColorVisitante { get; set; }
        public string? LogoVisitante { get; set; }
        public DateTime Fecha { get; set; }
        public string? Sede { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public string? NombreArbitro { get; set; }
        public bool EsMiEquipoLocal { get; set; }

        // Convocatoria fields
        public string EstadoConvocatoria { get; set; } = "SinConvocatoria";
        public int ConvocatoriaId { get; set; }
        public int EjtId { get; set; }
        public string EstadoConfirmacion { get; set; } = "Pendiente";
    }
}
