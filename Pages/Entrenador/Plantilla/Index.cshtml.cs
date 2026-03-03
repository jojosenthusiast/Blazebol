using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Entrenador.Plantilla
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string NombreEquipo { get; set; } = string.Empty;
        public int EquipoId { get; set; }
        public int? TorneoSeleccionado { get; set; }
        public List<TorneoDropdownItem> TorneosDisponibles { get; set; } = new();
        public List<PlantillaItemViewModel> Jugadores { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? torneoId, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            // Obtener entrenador del usuario
            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Entrenador == null)
                return RedirectToPage("/Error");

            // Obtener equipo del entrenador (propietario)
            var equipoEntrenador = await _context.EquiposEntrenadores
                .Include(ee => ee.Equipo)
                .Where(ee => ee.EntrenadorId == persona.Entrenador.Id && ee.Activo)
                .OrderByDescending(ee => ee.EsPropietario)
                .FirstOrDefaultAsync();

            if (equipoEntrenador == null)
            {
                ErrorMessage = "No tienes un equipo asignado.";
                return Page();
            }

            EquipoId = equipoEntrenador.EquipoId;
            NombreEquipo = equipoEntrenador.Equipo.Nombre;

            // Torneos donde el equipo está inscrito
            TorneosDisponibles = await _context.EquiposTorneos
                .Include(et => et.Torneo)
                .Where(et => et.EquipoId == EquipoId)
                .Select(et => new TorneoDropdownItem
                {
                    TorneoId = et.TorneoId,
                    Nombre = et.Torneo.Nombre,
                    Estado = et.Torneo.Estado
                })
                .OrderByDescending(t => t.TorneoId)
                .ToListAsync();

            // Seleccionar torneo: el que viene como parámetro o el primero disponible
            TorneoSeleccionado = torneoId ?? TorneosDisponibles.FirstOrDefault()?.TorneoId;

            if (TorneoSeleccionado.HasValue)
            {
                Jugadores = await _context.EquiposJugadoresTorneos
                    .Include(ejt => ejt.Jugador)
                        .ThenInclude(j => j.Persona)
                    .Where(ejt => ejt.EquipoId == EquipoId && ejt.TorneoId == TorneoSeleccionado.Value)
                    .OrderBy(ejt => ejt.NumeroCamiseta)
                    .Select(ejt => new PlantillaItemViewModel
                    {
                        Id = ejt.Id,
                        NombreCompleto = ejt.Jugador.Persona.Nombre + " " + ejt.Jugador.Persona.ApellidoPaterno,
                        Posicion = ejt.Jugador.PosicionPreferida ?? "N/A",
                        NumeroCamiseta = ejt.NumeroCamiseta,
                        Activo = ejt.Activo,
                        FechaInscripcion = ejt.FechaInscripcion
                    })
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDesactivarAsync(int id, int? torneoId)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            // Verificar que el registro pertenece al equipo del entrenador
            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Entrenador == null)
                return RedirectToPage("/Error");

            var equipoEntrenador = await _context.EquiposEntrenadores
                .Where(ee => ee.EntrenadorId == persona.Entrenador.Id && ee.Activo)
                .FirstOrDefaultAsync();

            if (equipoEntrenador == null)
                return RedirectToPage("/Error");

            var ejt = await _context.EquiposJugadoresTorneos.FindAsync(id);

            if (ejt == null || ejt.EquipoId != equipoEntrenador.EquipoId)
                return NotFound();

            ejt.Activo = false;
            ejt.FechaBaja = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Jugador EJT {Id} desactivado de plantilla por entrenador {EntrenadorId}",
                id, persona.Entrenador.Id);

            return RedirectToPage("/Entrenador/Plantilla/Index",
                new { torneoId, message = "Jugador dado de baja de la plantilla." });
        }
    }

    public class PlantillaItemViewModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Posicion { get; set; } = string.Empty;
        public int NumeroCamiseta { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaInscripcion { get; set; }
    }

    public class TorneoDropdownItem
    {
        public int TorneoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
