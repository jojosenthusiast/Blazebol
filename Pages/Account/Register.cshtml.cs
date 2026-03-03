using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterEspectadorViewModel InputEspectador { get; set; } = new();

        public string? TipoSeleccionado { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet(string? tipo)
        {
            TipoSeleccionado = tipo;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            TipoSeleccionado = "espectador";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authService.RegisterEspectadorAsync(InputEspectador);

                if (result.Success)
                {
                    _logger.LogInformation("Espectador {Email} registrado exitosamente", InputEspectador.Email);
                    return RedirectToPage("/Account/Login", new { message = "Cuenta creada exitosamente. Por favor, inicia sesión." });
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning("Error al registrar espectador {Email}: {Message}", InputEspectador.Email, result.Message);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar espectador {Email}", InputEspectador.Email);
                ErrorMessage = "Ocurrió un error al procesar su registro. Por favor, intente nuevamente.";
                return Page();
            }
        }
    }
}   