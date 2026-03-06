using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Pages.Calendario;

namespace ControlTorneosFutbol.Pages.Entrenador
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreCompleto { get; set; } = string.Empty;
        public string? NombreEquipo { get; set; }
        public int JugadoresEnPlantilla { get; set; }
        public int PartidosJugados { get; set; }
        public int Victorias { get; set; }
        public int Empates { get; set; }
        public int Derrotas { get; set; }
        public List<ProximoPartidoDashVM> ProximosPartidos { get; set; } = new();

        private static readonly string[] _torneoColors = { "#a3ff12", "#5ac8fa", "#ff9500", "#bf5af2", "#ff2d55", "#34c759", "#ffd60a" };
        private static string GetTorneoColor(int id) => _torneoColors[Math.Abs(id) % _torneoColors.Length];

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            if (!HttpContext.Session.IsInRole("Entrenador"))
            {
                return RedirectToPage("/Error", new { statusCode = 403 });
            }

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToPage("/Account/Login");
            }

            // Obtener información del entrenador
            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Entrenador == null)
            {
                return RedirectToPage("/Error");
            }

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";

            // Obtener equipo del entrenador
            var equipoEntrenador = await _context.EquiposEntrenadores
                .Include(ee => ee.Equipo)
                .Where(ee => ee.EntrenadorId == persona.Entrenador.Id && ee.Activo)
                .FirstOrDefaultAsync();

            if (equipoEntrenador != null)
            {
                NombreEquipo = equipoEntrenador.Equipo.Nombre;

                // Contar jugadores en plantilla (simplificado)
                JugadoresEnPlantilla = await _context.EquiposJugadoresTorneos
                    .Where(ejt => ejt.EquipoId == equipoEntrenador.EquipoId && ejt.Activo)
                    .CountAsync();
            }

            // Estadísticas (placeholder - implementar después)
            PartidosJugados = 0;
            Victorias = 0;
            Empates = 0;
            Derrotas = 0;

            // Próximos partidos del equipo
            if (equipoEntrenador != null)
            {
                var miEquipoId = equipoEntrenador.EquipoId;
                ProximosPartidos = await _context.Partidos
                    .Include(p => p.Torneo)
                    .Include(p => p.EquipoLocal)
                    .Include(p => p.EquipoVisitante)
                    .Where(p => (p.EquipoLocalId == miEquipoId || p.EquipoVisitanteId == miEquipoId)
                             && p.Estado == EstadosPartido.Programado
                             && p.Fecha >= DateTime.UtcNow)
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
                        Sede = p.Sede,
                        EsMiPartido = true
                    })
                    .ToListAsync();

                foreach (var p in ProximosPartidos)
                    p.ColorTorneo = GetTorneoColor(p.TorneoId);
            }

            return Page();
        }
    }
}