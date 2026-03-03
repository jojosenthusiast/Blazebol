using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Account
{
    public class RegisterArbitroModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterArbitroModel> _logger;

        public RegisterArbitroModel(IAuthService authService, ILogger<RegisterArbitroModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterArbitroViewModel Input { get; set; } = new();

        // Checkbox group values (name="nivelesLicencia")
        [BindProperty]
        public List<string> NivelesLicencia { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (!NivelesLicencia.Any())
            {
                ErrorMessage = "Debes seleccionar al menos un nivel de licencia.";
                return Page();
            }

            // Store selected levels as comma-separated string
            Input.CategoriasArbitra = string.Join(",", NivelesLicencia);

            try
            {
                var result = await _authService.RegisterArbitroAsync(Input);

                if (result.Success)
                {
                    _logger.LogInformation("Árbitro {Email} registrado (Pendiente de aprobación)", Input.Email);
                    return RedirectToPage("/Account/Login",
                        new { message = "Solicitud de registro enviada. Un administrador revisará tu cuenta próximamente." });
                }
                else
                {
                    ErrorMessage = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar árbitro {Email}", Input.Email);
                ErrorMessage = "Ocurrió un error al procesar su registro. Por favor, intente nuevamente.";
                return Page();
            }
        }
    }
}
