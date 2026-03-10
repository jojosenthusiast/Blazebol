using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Models.ViewModels;

namespace ControlTorneosFutbol.Pages.Torneos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? Estado { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoriaId { get; set; }

        public List<TorneoPublicoViewModel> Torneos { get; set; } = new();
        public List<Data.Entities.Categoria> Categorias { get; set; } = new();
        public bool IsAuthenticated { get; set; }
        public string DashboardPage { get; set; } = "/Account/Login";

        public async Task OnGetAsync()
        {
            IsAuthenticated = HttpContext.Session.IsAuthenticated();
            var role = HttpContext.Session.GetUserRole();
            if (IsAuthenticated && !string.IsNullOrEmpty(role))
            {
                DashboardPage = role switch
                {
                    "Administrador" => "/Admin/Dashboard",
                    "Jugador"       => "/Jugador/Dashboard",
                    "Entrenador"    => "/Entrenador/Dashboard",
                    "Arbitro"       => "/Arbitro/Dashboard",
                    "Espectador"    => "/Espectador/Index",
                    _               => "/Account/Login"
                };
            }

            Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();

            var query = _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Categoria)
                .Include(t => t.EquiposTorneos)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(Estado))
                query = query.Where(t => t.Estado == Estado);
            else
                query = query.Where(t => t.Estado != "Finalizado"); // por defecto excluir finalizados

            if (!string.IsNullOrEmpty(Busqueda))
                query = query.Where(t => t.Nombre.ToLower().Contains(Busqueda.ToLower()));

            if (CategoriaId.HasValue)
                query = query.Where(t => t.CategoriaId == CategoriaId.Value);

            Torneos = await query
                .OrderByDescending(t => t.Estado == "Activo")
                .ThenBy(t => t.FechaInicio)
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
