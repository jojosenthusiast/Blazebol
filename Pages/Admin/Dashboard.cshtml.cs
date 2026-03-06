using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Pages.Calendario;

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
        public List<ProximoPartidoDashVM> ProximosPartidos { get; set; } = new();

        private static readonly string[] _torneoColors = { "#a3ff12", "#5ac8fa", "#ff9500", "#bf5af2", "#ff2d55", "#34c759", "#ffd60a" };
        private static string GetTorneoColor(int id) => _torneoColors[Math.Abs(id) % _torneoColors.Length];

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

            ProximosPartidos = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Where(p => p.Estado == EstadosPartido.Programado && p.Fecha >= DateTime.UtcNow)
                .OrderBy(p => p.Fecha)
                .Take(5)
                .Select(p => new ProximoPartidoDashVM
                {
                    Id = p.Id,
                    Fecha = p.Fecha,
                    NombreTorneo = p.Torneo.Nombre,
                    TorneoId = p.TorneoId,
                    NombreLocal = p.EquipoLocal.Nombre,
                    NombreVisitante = p.EquipoVisitante.Nombre,
                    AbrevLocal = p.EquipoLocal.Abreviacion ?? p.EquipoLocal.Nombre.Substring(0, Math.Min(3, p.EquipoLocal.Nombre.Length)).ToUpper(),
                    AbrevVisitante = p.EquipoVisitante.Abreviacion ?? p.EquipoVisitante.Nombre.Substring(0, Math.Min(3, p.EquipoVisitante.Nombre.Length)).ToUpper(),
                    Estado = p.Estado,
                    Sede = p.Sede
                })
                .ToListAsync();

            // Asignar colores por torneo
            foreach (var p in ProximosPartidos)
                p.ColorTorneo = GetTorneoColor(p.TorneoId);

            return Page();
        }
    }
}