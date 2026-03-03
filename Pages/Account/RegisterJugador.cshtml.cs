using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Account
{
    public class RegisterJugadorModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterJugadorModel> _logger;

        public RegisterJugadorModel(IAuthService authService, ILogger<RegisterJugadorModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterJugadorViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authService.RegisterJugadorAsync(Input);

                if (result.Success)
                {
                    _logger.LogInformation("Jugador {Email} registrado exitosamente", Input.Email);
                    return RedirectToPage("/Account/Login", new { message = "Cuenta de jugador creada exitosamente. Por favor, inicia sesión." });
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning("Error al registrar jugador {Email}: {Message}", Input.Email, result.Message);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar jugador {Email}", Input.Email);
                ErrorMessage = "Ocurrió un error al procesar su registro. Por favor, intente nuevamente.";
                return Page();
            }
        }
    }
}