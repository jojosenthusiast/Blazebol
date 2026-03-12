using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Arbitro.Partidos
{
    public class VerificacionModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPartidoEnVivoService _enVivoService;
        private readonly INotificacionService _notificaciones;

        public VerificacionModel(ApplicationDbContext context, IPartidoEnVivoService enVivoService, INotificacionService notificaciones)
        {
            _context = context;
            _enVivoService = enVivoService;
            _notificaciones = notificaciones;
        }

        public int PartidoId { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public DateTime FechaPartido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Sede { get; set; }
        public string? ErrorMessage { get; set; }

        public List<ParticipanteViewModel> JugadoresLocal { get; set; } = new();
        public List<ParticipanteViewModel> JugadoresVisitante { get; set; } = new();
        public List<ParticipanteViewModel> EntrenadoresLocal { get; set; } = new();
        public List<ParticipanteViewModel> EntrenadoresVisitante { get; set; } = new();

        public List<CheckPrePartidoViewModel> Checks { get; set; } = new();
        public bool PuedeProceeder => Checks.All(c => c.Pasa);
        public int MinimoJugadores { get; set; } = 11;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            // Get árbitro from session
            var persona = await _context.Personas
                .Include(p => p.Arbitro)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Arbitro == null)
                return RedirectToPage("/Error");

            if (persona.Arbitro.EstadoAprobacion != EstadosAprobacionArbitro.Aprobado)
                return RedirectToPage("/Arbitro/Partidos/Index");

            // Load the match
            var partido = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partido == null)
                return NotFound();

            // Verify this árbitro is assigned to the match
            if (partido.ArbitroId != persona.Arbitro.Id)
                return RedirectToPage("/Arbitro/Partidos/Index");

            PartidoId = partido.Id;
            NombreLocal = partido.EquipoLocal.Nombre;
            NombreVisitante = partido.EquipoVisitante.Nombre;
            NombreTorneo = partido.Torneo.Nombre;
            FechaPartido = partido.Fecha;
            Estado = partido.Estado;
            Sede = partido.Sede;

            if (partido.Estado != EstadosPartido.Programado)
            {
                ErrorMessage = $"La verificación de identidades solo está disponible para partidos en estado \"Programado\". Este partido está en estado \"{partido.Estado}\".";
                return Page();
            }

            // Load active players for local team in this torneo
            JugadoresLocal = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoLocalId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    Rol = ejt.Jugador.PosicionPreferida ?? "—",
                    Activo = ejt.Activo
                })
                .ToListAsync();

            // Load active players for visitante team in this torneo
            JugadoresVisitante = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoVisitanteId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    Rol = ejt.Jugador.PosicionPreferida ?? "—",
                    Activo = ejt.Activo
                })
                .ToListAsync();

            // Load active coaches for local team
            EntrenadoresLocal = await _context.EquiposEntrenadores
                .Include(ee => ee.Entrenador).ThenInclude(e => e.Persona)
                .Where(ee => ee.EquipoId == partido.EquipoLocalId && ee.Activo)
                .OrderBy(ee => ee.Rol)
                .Select(ee => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = null,
                    Rol = ee.Rol,
                    Activo = ee.Activo
                })
                .ToListAsync();

            // Load active coaches for visitante team
            EntrenadoresVisitante = await _context.EquiposEntrenadores
                .Include(ee => ee.Entrenador).ThenInclude(e => e.Persona)
                .Where(ee => ee.EquipoId == partido.EquipoVisitanteId && ee.Activo)
                .OrderBy(ee => ee.Rol)
                .Select(ee => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = null,
                    Rol = ee.Rol,
                    Activo = ee.Activo
                })
                .ToListAsync();

            // Cargar configuración del torneo para obtener mínimo de jugadores
            var config = await _context.ConfiguracionesTorneo
                .FirstOrDefaultAsync(c => c.TorneoId == partido.TorneoId);

            MinimoJugadores = config?.MinimoJugadoresTitulares ?? 11;

            // Calcular checks pre-partido
            bool cuorumLocal = JugadoresLocal.Count >= MinimoJugadores;
            bool cuorumVisitante = JugadoresVisitante.Count >= MinimoJugadores;
            bool entrenadorLocal = EntrenadoresLocal.Any();
            bool entrenadorVisitante = EntrenadoresVisitante.Any();

            Checks = new List<CheckPrePartidoViewModel>
            {
                new()
                {
                    Nombre = "Estado del partido",
                    Pasa = true,
                    Descripcion = "El partido está en estado Programado y aún no ha iniciado"
                },
                new()
                {
                    Nombre = $"Cuórum equipo local — {NombreLocal}",
                    Pasa = cuorumLocal,
                    Descripcion = cuorumLocal
                        ? $"{JugadoresLocal.Count} jugadores activos inscritos (mínimo requerido: {MinimoJugadores})"
                        : $"Solo {JugadoresLocal.Count} jugadores activos inscritos — se requieren al menos {MinimoJugadores}"
                },
                new()
                {
                    Nombre = $"Cuórum equipo visitante — {NombreVisitante}",
                    Pasa = cuorumVisitante,
                    Descripcion = cuorumVisitante
                        ? $"{JugadoresVisitante.Count} jugadores activos inscritos (mínimo requerido: {MinimoJugadores})"
                        : $"Solo {JugadoresVisitante.Count} jugadores activos inscritos — se requieren al menos {MinimoJugadores}"
                },
                new()
                {
                    Nombre = $"Cuerpo técnico local — {NombreLocal}",
                    Pasa = entrenadorLocal,
                    Descripcion = entrenadorLocal
                        ? $"{EntrenadoresLocal.Count} entrenador(es) asignado(s)"
                        : "Sin entrenador asignado — el equipo no puede participar sin cuerpo técnico"
                },
                new()
                {
                    Nombre = $"Cuerpo técnico visitante — {NombreVisitante}",
                    Pasa = entrenadorVisitante,
                    Descripcion = entrenadorVisitante
                        ? $"{EntrenadoresVisitante.Count} entrenador(es) asignado(s)"
                        : "Sin entrenador asignado — el equipo no puede participar sin cuerpo técnico"
                }
            };

            return Page();
        }

        public async Task<IActionResult> OnPostIniciarPartidoAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Arbitro)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Arbitro == null) return RedirectToPage("/Error");

            var ok = await _enVivoService.IniciarPartidoAsync(id, persona.Arbitro.Id, _notificaciones);
            if (!ok)
            {
                return RedirectToPage(new { id });
            }

            return RedirectToPage("/Arbitro/Partidos/EnVivo", new { partidoId = id });
        }
    }

    public class ParticipanteViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public int? NumeroCamiseta { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CheckPrePartidoViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public bool Pasa { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
