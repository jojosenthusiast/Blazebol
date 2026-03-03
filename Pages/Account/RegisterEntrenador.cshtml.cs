using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Account
{
    public class RegisterEntrenadorModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<RegisterEntrenadorModel> _logger;

        public RegisterEntrenadorModel(IAuthService authService, IWebHostEnvironment env, ILogger<RegisterEntrenadorModel> logger)
        {
            _authService = authService;
            _env = env;
            _logger = logger;
        }

        [BindProperty]
        public RegisterEntrenadorViewModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? LogoFile { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Handle shield/logo upload
            if (LogoFile != null && LogoFile.Length > 0)
            {
                var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/svg+xml", "image/webp" };
                if (!allowed.Contains(LogoFile.ContentType.ToLower()))
                {
                    ErrorMessage = "El escudo debe ser una imagen (PNG, JPG, SVG o WebP).";
                    return Page();
                }
                if (LogoFile.Length > 2 * 1024 * 1024)
                {
                    ErrorMessage = "El escudo no puede superar 2 MB.";
                    return Page();
                }

                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "escudos");
                Directory.CreateDirectory(uploadDir);
                var ext = Path.GetExtension(LogoFile.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);

                using var stream = System.IO.File.Create(filePath);
                await LogoFile.CopyToAsync(stream);

                Input.LogoPath = $"/uploads/escudos/{fileName}";
            }

            try
            {
                var result = await _authService.RegisterEntrenadorAsync(Input);

                if (result.Success)
                {
                    _logger.LogInformation("Entrenador {Email} y equipo {Equipo} registrados exitosamente",
                        Input.Email, Input.NombreEquipo);
                    return RedirectToPage("/Account/Login",
                        new { message = "Cuenta de entrenador y equipo creados exitosamente. Por favor, inicia sesión." });
                }
                else
                {
                    ErrorMessage = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar entrenador {Email}", Input.Email);
                ErrorMessage = "Ocurrió un error al procesar su registro. Por favor, intente nuevamente.";
                return Page();
            }
        }
    }
}
