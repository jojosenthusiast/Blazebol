using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Entrenador
{
    public class MiEquipoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MiEquipoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreEquipo { get; set; } = string.Empty;
        public string? Sede { get; set; }
        public string? ColorPrimario { get; set; }
        public string? ColorSecundario { get; set; }
        public string? ErrorMessage { get; set; }

        public List<TorneoEquipoViewModel> Torneos { get; set; } = new();
        public List<TecnicoViewModel> CuerpoTecnico { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Entrenador == null)
                return RedirectToPage("/Error");

            var equipoEntrenador = await _context.EquiposEntrenadores
                .Include(ee => ee.Equipo)
                    .ThenInclude(e => e.EquiposTorneos)
                        .ThenInclude(et => et.Torneo)
                .Where(ee => ee.EntrenadorId == persona.Entrenador.Id && ee.Activo)
                .OrderByDescending(ee => ee.EsPropietario)
                .FirstOrDefaultAsync();

            if (equipoEntrenador == null)
            {
                ErrorMessage = "No tienes un equipo asignado. Contacta al administrador.";
                return Page();
            }

            var equipo = equipoEntrenador.Equipo;
            NombreEquipo = equipo.Nombre;
            Sede = equipo.Sede;
            ColorPrimario = equipo.ColorPrimario;
            ColorSecundario = equipo.ColorSecundario;

            Torneos = equipo.EquiposTorneos
                .Select(et => new TorneoEquipoViewModel
                {
                    TorneoId = et.TorneoId,
                    NombreTorneo = et.Torneo.Nombre,
                    EstadoTorneo = et.Torneo.Estado,
                    FechaInscripcion = et.FechaInscripcion
                })
                .OrderByDescending(t => t.FechaInscripcion)
                .ToList();

            CuerpoTecnico = await _context.EquiposEntrenadores
                .Include(ee => ee.Entrenador).ThenInclude(e => e.Persona)
                .Where(ee => ee.EquipoId == equipo.Id && ee.Activo)
                .Select(ee => new TecnicoViewModel
                {
                    NombreCompleto = $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}",
                    Rol = ee.Rol,
                    EsPropietario = ee.EsPropietario
                })
                .OrderByDescending(t => t.EsPropietario)
                .ToListAsync();

            return Page();
        }
    }

    public class TorneoEquipoViewModel
    {
        public int TorneoId { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string EstadoTorneo { get; set; } = string.Empty;
        public DateTime FechaInscripcion { get; set; }
    }

    public class TecnicoViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool EsPropietario { get; set; }
    }
}
