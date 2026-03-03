using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class CreateModel : PageModel
    {
        private readonly ITorneoService _torneoService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ITorneoService torneoService, ILogger<CreateModel> logger)
        {
            _torneoService = torneoService;
            _logger = logger;
        }

        [BindProperty]
        public TorneoViewModel Input { get; set; } = new();

        public List<Liga> Ligas { get; set; } = new();
        public List<Temporada> Temporadas { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                var result = await _torneoService.CreateTorneoAsync(Input);

                if (result.Success && result.TorneoId.HasValue)
                {
                    _logger.LogInformation("Torneo {Nombre} creado exitosamente con ID {TorneoId}", Input.Nombre, result.TorneoId);
                    return RedirectToPage("/Admin/Torneos/Details", new { id = result.TorneoId, message = "Torneo creado exitosamente" });
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning("Error al crear torneo {Nombre}: {Message}", Input.Nombre, result.Message);
                    await LoadSelectListsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear torneo {Nombre}", Input.Nombre);
                ErrorMessage = "Ocurrió un error al crear el torneo. Por favor, intente nuevamente.";
                await LoadSelectListsAsync();
                return Page();
            }
        }

        private async Task LoadSelectListsAsync()
        {
            Ligas = await _torneoService.GetLigasActivasAsync();
            Temporadas = await _torneoService.GetTemporadasActivasAsync();
            Categorias = await _torneoService.GetCategoriasAsync();
        }
    }
}