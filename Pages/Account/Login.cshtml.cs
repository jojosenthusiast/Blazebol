using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet(string? message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _authService.LoginAsync(Input, ipAddress);

                if (result.Success && result.UserId.HasValue && !string.IsNullOrEmpty(result.Rol))
                {
                    // Obtener nombre del usuario para la sesión
                    var userName = Input.Email.Split('@')[0]; // Temporal, mejorar después

                    // Establecer sesión
                    HttpContext.Session.SetUserSession(
                        result.UserId.Value,
                        result.Rol,
                        userName,
                        Input.Email
                    );

                    _logger.LogInformation("Usuario {Email} inició sesión exitosamente", Input.Email);

                    // Redirigir según rol
                    return result.Rol switch
                    {
                        "Administrador" => RedirectToPage("/Admin/Dashboard"),
                        "Jugador" => RedirectToPage("/Jugador/Dashboard"),
                        "Entrenador" => RedirectToPage("/Entrenador/Dashboard"),
                        "Arbitro" => RedirectToPage("/Arbitro/Dashboard"),
                        "Espectador" => RedirectToPage("/Espectador/Index"),
                        _ => RedirectToPage("/Index")
                    };
                }
                else
                {
                    ErrorMessage = result.Message;
                    _logger.LogWarning("Intento de login fallido para {Email}: {Message}", Input.Email, result.Message);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar login para {Email}", Input.Email);
                ErrorMessage = "Ocurrió un error al procesar su solicitud. Por favor, intente nuevamente.";
                return Page();
            }
        }
    }
}