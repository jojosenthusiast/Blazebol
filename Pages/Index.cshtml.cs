using Microsoft.AspNetCore.Mvc.RazorPages;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages
{
    public class IndexModel : PageModel
    {
        public bool IsAuthenticated { get; set; }
        public string? UserRole { get; set; }
        public string DashboardPage { get; set; } = "/Account/Login";

        public void OnGet()
        {
            IsAuthenticated = HttpContext.Session.IsAuthenticated();
            UserRole = HttpContext.Session.GetUserRole();

            if (IsAuthenticated && !string.IsNullOrEmpty(UserRole))
            {
                DashboardPage = UserRole switch
                {
                    "Administrador" => "/Admin/Dashboard",
                    "Jugador" => "/Jugador/Dashboard",
                    "Entrenador" => "/Entrenador/Dashboard",
                    "Arbitro" => "/Arbitro/Dashboard",
                    "Espectador" => "/Espectador/Index",
                    _ => "/Account/Login"
                };
            }
        }
    }
}
