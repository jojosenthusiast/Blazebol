using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string UserName { get; set; } = string.Empty;
        public int TotalTorneos { get; set; }
        public int TorneosActivos { get; set; }
        public int TotalEquipos { get; set; }
        public int TotalJugadores { get; set; }
        public int TotalArbitros { get; set; }
        public int ArbitrosPendientesAprobacion { get; set; }
        public int AccesosHoy { get; set; }
        public int IntentosFallidos { get; set; }
        public int UsuariosBloqueados { get; set; }
        public List<TorneoItemViewModel> UltimosTorneos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar autenticación
            if (!HttpContext.Session.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            // Verificar rol de administrador
            if (!HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Error", new { statusCode = 403 });
            }

            UserName = HttpContext.Session.GetUserName() ?? "Administrador";
            var hoy = DateTime.UtcNow.Date;
            var registrosHoy = await _context.AuditoriaAccesos
                .Where(a => a.FechaIntento >= hoy)
                .ToListAsync();

            // Cargar estadísticas
            TotalTorneos = await _context.Torneos.CountAsync();
            TorneosActivos = await _context.Torneos.CountAsync(t => t.Estado == "Activo");
            TotalEquipos = await _context.Equipos.CountAsync(e => e.Activo);
            TotalJugadores = await _context.Jugadores.CountAsync();
            TotalArbitros = await _context.Arbitros.CountAsync();
            ArbitrosPendientesAprobacion = await _context.Arbitros
                .CountAsync(a => a.EstadoAprobacion == "Pendiente");

            // Cargar últimos torneos
            UltimosTorneos = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.FechaCreacion)
                .Take(5)
                .Select(t => new TorneoItemViewModel
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    NombreLiga = t.Liga.Nombre,
                    NombreTemporada = t.Temporada.Nombre,
                    NombreCategoria = t.Categoria.Nombre,
                    Formato = t.Formato,
                    Estado = t.Estado,
                    FechaInicio = t.FechaInicio,
                    FechaFin = t.FechaFin,
                    NumeroEquiposMaximo = t.NumeroEquiposMaximo,
                    PuedeEditar = t.Estado != "Finalizado"
                })
                .ToListAsync();

            return Page();
        }
    }
}