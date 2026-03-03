using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class EditModel : PageModel
    {
        private readonly ITorneoService _torneoService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ITorneoService torneoService, ApplicationDbContext context, ILogger<EditModel> logger)
        {
            _torneoService = torneoService;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public TorneoViewModel Input { get; set; } = new();

        public List<Liga> Ligas { get; set; } = new();
        public List<Temporada> Temporadas { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();
        public bool EsFinalizado { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            var torneo = await _torneoService.GetTorneoByIdAsync(id);
            if (torneo == null)
                return NotFound();

            EsFinalizado = torneo.Estado == EstadosTorneo.Finalizado;

            Input = new TorneoViewModel
            {
                Id = torneo.Id,
                Nombre = torneo.Nombre,
                LigaId = torneo.LigaId,
                TemporadaId = torneo.TemporadaId,
                CategoriaId = torneo.CategoriaId,
                Formato = torneo.Formato,
                Estado = torneo.Estado,
                FechaInicio = torneo.FechaInicio,
                FechaFin = torneo.FechaFin,
                NumeroEquiposMinimo = torneo.NumeroEquiposMinimo,
                NumeroEquiposMaximo = torneo.NumeroEquiposMaximo
            };

            await LoadSelectListsAsync(torneo.LigaId, torneo.TemporadaId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!Input.Id.HasValue)
                return NotFound();

            // Verificar si puede editar (servidor)
            var canEdit = await _torneoService.CanEditTorneoAsync(Input.Id.Value);
            if (!canEdit)
            {
                EsFinalizado = true;
                ErrorMessage = "No se puede editar un torneo que ya está finalizado.";
                await LoadSelectListsAsync(Input.LigaId, Input.TemporadaId);
                return Page();
            }

            // Validación de fechas (además de DataAnnotations)
            if (Input.FechaFin <= Input.FechaInicio)
            {
                ModelState.AddModelError("Input.FechaFin",
                    "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            // Validar que no se pueda "auto-seleccionar" Finalizado sin confirmar
            // (el admin debe usar el botón Finalizar, no el dropdown)
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync(Input.LigaId, Input.TemporadaId);
                return Page();
            }

            try
            {
                var result = await _torneoService.UpdateTorneoAsync(Input.Id.Value, Input);
                if (result.Success)
                {
                    _logger.LogInformation("Torneo {Id} actualizado por administrador", Input.Id);
                    return RedirectToPage("/Admin/Torneos/Details",
                        new { id = Input.Id.Value, message = "Torneo actualizado exitosamente." });
                }

                ErrorMessage = result.Message;
                await LoadSelectListsAsync(Input.LigaId, Input.TemporadaId);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar torneo {Id}", Input.Id);
                ErrorMessage = "Ocurrió un error inesperado al guardar los cambios. Por favor intente nuevamente.";
                await LoadSelectListsAsync(Input.LigaId, Input.TemporadaId);
                return Page();
            }
        }

        // Carga ligas y temporadas incluyendo siempre las actualmente asignadas al torneo,
        // incluso si fueron desactivadas posteriormente — evita que el form quede sin opciones.
        private async Task LoadSelectListsAsync(int currentLigaId = 0, int currentTemporadaId = 0)
        {
            Ligas = await _context.Ligas
                .Where(l => l.Activa || l.Id == currentLigaId)
                .OrderBy(l => l.Nombre)
                .ToListAsync();

            Temporadas = await _context.Temporadas
                .Where(t => t.Activa || t.Id == currentTemporadaId)
                .OrderByDescending(t => t.Anio)
                .ThenBy(t => t.Nombre)
                .ToListAsync();

            Categorias = await _torneoService.GetCategoriasAsync();
        }
    }
}
