using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Models.ViewModels;

namespace ControlTorneosFutbol.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool IsAuthenticated { get; set; }
        public string? UserRole { get; set; }
        public string DashboardPage { get; set; } = "/Account/Login";

        // Stats del hero
        public int TotalTorneos { get; set; }
        public int TotalEquipos { get; set; }
        public int TotalJugadores { get; set; }

        // Torneos destacados (máx. 6)
        public List<TorneoPublicoViewModel> TorneosDestacados { get; set; } = new();

        public async Task OnGetAsync()
        {
            IsAuthenticated = HttpContext.Session.IsAuthenticated();
            UserRole = HttpContext.Session.GetUserRole();

            if (IsAuthenticated && !string.IsNullOrEmpty(UserRole))
            {
                DashboardPage = UserRole switch
                {
                    "Administrador" => "/Admin/Dashboard",
                    "Jugador"       => "/Jugador/Dashboard",
                    "Entrenador"    => "/Entrenador/Dashboard",
                    "Arbitro"       => "/Arbitro/Dashboard",
                    "Espectador"    => "/Espectador/Index",
                    _               => "/Account/Login"
                };
            }

            // Stats
            TotalTorneos   = await _context.Torneos.CountAsync(t => t.Estado == "Activo");
            TotalEquipos   = await _context.Equipos.CountAsync(e => e.Activo);
            TotalJugadores = await _context.Jugadores.CountAsync();

            // Torneos destacados: activos primero, luego próximos
            TorneosDestacados = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Categoria)
                .Include(t => t.EquiposTorneos)
                .Where(t => t.Estado == "Activo" || t.Estado == "Borrador")
                .OrderByDescending(t => t.Estado == "Activo")
                .ThenBy(t => t.FechaInicio)
                .Take(6)
                .Select(t => new TorneoPublicoViewModel
                {
                    Id               = t.Id,
                    Nombre           = t.Nombre,
                    FechaInicio      = t.FechaInicio,
                    FechaFin         = t.FechaFin,
                    Estado           = t.Estado,
                    LigaNombre       = t.Liga.Nombre,
                    CategoriaNombre  = t.Categoria.Nombre,
                    ImagenUrl        = t.ImagenUrl,
                    LogoUrl          = t.LogoUrl,
                    EquiposInscritos = t.EquiposTorneos.Count,
                    EquiposMaximo    = t.NumeroEquiposMaximo
                })
                .ToListAsync();
        }
    }
}
