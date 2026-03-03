using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class IndexModel : PageModel
    {
        private readonly ITorneoService _torneoService;
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITorneoService torneoService,
            ApplicationDbContext context,
            IPdfService pdfService,
            ILogger<IndexModel> logger)
        {
            _torneoService = torneoService;
            _context = context;
            _pdfService = pdfService;
            _logger = logger;
        }

        public List<TorneoItemViewModel> Torneos { get; set; } = new();
        public string? FiltroEstado { get; set; }
        public int? FiltroAnio { get; set; }
        public int? FiltroCategoriaId { get; set; }
        public List<int> AniosDisponibles { get; set; } = new();
        public Dictionary<int, string> CategoriasDisponibles { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? estado, int? anio, int? categoriaId, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            FiltroEstado = estado;
            FiltroAnio = anio;
            FiltroCategoriaId = categoriaId;

            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            // Cargar torneos con filtros
            var torneos = await _torneoService.GetTorneosAsync(estado, anio, categoriaId);

            Torneos = torneos.Select(t => new TorneoItemViewModel
            {
                Id = t.Id,
                Nombre = t.Nombre,
                NombreLiga = t.Liga.Nombre,
                NombreTemporada = t.Temporada.Nombre,
                NombreCategoria = t.Categoria.Nombre,
                Formato = t.Formato,
                Estado = t.Estado,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin,
                NumeroEquiposMaximo = t.NumeroEquiposMaximo,
                NumeroEquiposInscritos = t.EquiposTorneos.Count,
                PuedeEditar = t.Estado != "Finalizado"
            }).ToList();

            // Cargar datos para filtros
            var temporadas = await _torneoService.GetTemporadasActivasAsync();
            AniosDisponibles = temporadas.Select(t => t.Anio).Distinct().OrderByDescending(a => a).ToList();

            var categorias = await _torneoService.GetCategoriasAsync();
            CategoriasDisponibles = categorias.ToDictionary(c => c.Id, c => c.Nombre);

            return Page();
        }

        public async Task<IActionResult> OnPostDesactivarAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                var result = await _torneoService.DesactivarTorneoAsync(id);

                if (result.Success)
                {
                    _logger.LogInformation("Torneo {TorneoId} finalizado por administrador", id);
                    return RedirectToPage("/Admin/Torneos/Index", new { message = "Torneo finalizado exitosamente" });
                }
                else
                {
                    ErrorMessage = result.Message;
                    return await OnGetAsync(null, null, null, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar torneo {TorneoId}", id);
                ErrorMessage = "Ocurrió un error al finalizar el torneo";
                return await OnGetAsync(null, null, null, null);
            }
        }

        public async Task<IActionResult> OnPostExportarPDFAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            // Obtener torneos directamente del contexto
            var torneos = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            var pdfBytes = _pdfService.GenerateTorneosReport(torneos);

            return File(pdfBytes, "application/pdf", $"Torneos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}