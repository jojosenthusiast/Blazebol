using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlTorneosFutbol.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Obtener información del usuario antes de limpiar sesión
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userName = HttpContext.Session.GetString("UserName");

            // Limpiar TODA la sesión
            HttpContext.Session.Clear();

            // Log de auditoría
            _logger.LogInformation(
                "Logout exitoso - Usuario: {Email} ({Name}), Rol: {Role}, IP: {IP}",
                userEmail ?? "Desconocido",
                userName ?? "Desconocido",
                userRole ?? "Desconocido",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0"
            );

            // Redirigir a página principal
            return RedirectToPage("/Index");
        }
    }
}