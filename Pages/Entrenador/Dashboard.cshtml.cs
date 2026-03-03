using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

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
        public List<string> ProximosPartidos { get; set; } = new();

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

            ProximosPartidos = new List<string>();

            return Page();
        }
    }
}