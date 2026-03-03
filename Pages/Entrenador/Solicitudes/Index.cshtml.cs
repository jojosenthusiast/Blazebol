using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Entrenador.Solicitudes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreEquipo { get; set; } = string.Empty;
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        // Solicitudes recibidas de jugadores (EsInvitacion=false, Pendiente)
        public List<SolicitudRecibidaViewModel> SolicitudesRecibidas { get; set; } = new();
        // Invitaciones enviadas por el coach (EsInvitacion=true)
        public List<InvitacionEnviadaViewModel> InvitacionesEnviadas { get; set; } = new();
        // Historial (aceptadas/rechazadas)
        public List<SolicitudRecibidaViewModel> Historial { get; set; } = new();

        private async Task<(Data.Entities.Entrenador? entrenador, int equipoId)?> ObtenerEntrenadorEquipoAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return null;

            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Entrenador == null) return null;

            var ee = await _context.EquiposEntrenadores
                .Where(e => e.EntrenadorId == persona.Entrenador.Id && e.Activo)
                .OrderByDescending(e => e.EsPropietario)
                .FirstOrDefaultAsync();

            if (ee == null) return null;

            return (persona.Entrenador, ee.EquipoId);
        }

        public async Task<IActionResult> OnGetAsync(string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            var result = await ObtenerEntrenadorEquipoAsync();
            if (result == null)
            {
                ErrorMessage = "No tienes un equipo asignado.";
                return Page();
            }

            var (_, equipoId) = result.Value;

            var equipo = await _context.Equipos.FindAsync(equipoId);
            NombreEquipo = equipo?.Nombre ?? string.Empty;

            var todasSolicitudes = await _context.SolicitudesJugadorEquipo
                .Include(s => s.Jugador).ThenInclude(j => j.Persona).ThenInclude(p => p.Usuario)
                .Include(s => s.Torneo)
                .Where(s => s.EquipoId == equipoId)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            SolicitudRecibidaViewModel MapSolicitud(SolicitudJugadorEquipo s)
            {
                var p = s.Jugador.Persona;
                int? edad = p.FechaNacimiento.HasValue
                    ? (int)((DateTime.UtcNow - p.FechaNacimiento.Value).TotalDays / 365.25)
                    : null;
                return new SolicitudRecibidaViewModel
                {
                    Id = s.Id,
                    NombreJugador = $"{p.Nombre} {p.ApellidoPaterno}",
                    NombreTorneo = s.Torneo?.Nombre ?? "Sin torneo específico",
                    Mensaje = s.MensajeJugador,
                    Fecha = s.FechaSolicitud,
                    Estado = s.Estado,
                    Email = p.Usuario?.Email,
                    Telefono = p.Telefono,
                    Posicion = s.Jugador.PosicionPreferida,
                    Edad = edad
                };
            }

            SolicitudesRecibidas = todasSolicitudes
                .Where(s => !s.EsInvitacion && s.Estado == EstadosSolicitud.Pendiente)
                .Select(s => MapSolicitud(s)).ToList();

            InvitacionesEnviadas = todasSolicitudes
                .Where(s => s.EsInvitacion)
                .Select(s => new InvitacionEnviadaViewModel
                {
                    Id = s.Id,
                    NombreJugador = $"{s.Jugador.Persona.Nombre} {s.Jugador.Persona.ApellidoPaterno}",
                    NombreTorneo = s.Torneo?.Nombre ?? "Sin torneo específico",
                    Fecha = s.FechaSolicitud,
                    Estado = s.Estado
                }).ToList();

            Historial = todasSolicitudes
                .Where(s => !s.EsInvitacion && s.Estado != EstadosSolicitud.Pendiente)
                .Select(s => MapSolicitud(s)).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAceptarAsync(int id, int numeroCamiseta)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var result = await ObtenerEntrenadorEquipoAsync();
            if (result == null)
                return RedirectToPage("/Account/Login");

            var (entrenador, equipoId) = result.Value;

            var solicitud = await _context.SolicitudesJugadorEquipo
                .FirstOrDefaultAsync(s => s.Id == id && s.EquipoId == equipoId
                                       && !s.EsInvitacion && s.Estado == EstadosSolicitud.Pendiente);

            if (solicitud == null)
                return RedirectToPage(new { message = "Solicitud no encontrada." });

            // Validate jersey number uniqueness
            if (solicitud.TorneoId.HasValue)
            {
                bool camisetaOcupada = await _context.EquiposJugadoresTorneos
                    .AnyAsync(ejt => ejt.EquipoId == equipoId
                                  && ejt.TorneoId == solicitud.TorneoId
                                  && ejt.NumeroCamiseta == numeroCamiseta
                                  && ejt.Activo);
                if (camisetaOcupada)
                {
                    ErrorMessage = $"El número de camiseta {numeroCamiseta} ya está en uso en este torneo.";
                    return await OnGetAsync(null);
                }

                // Validate player not already in a team in this torneo
                bool yaEnTorneo = await _context.EquiposJugadoresTorneos
                    .AnyAsync(ejt => ejt.JugadorId == solicitud.JugadorId
                                  && ejt.TorneoId == solicitud.TorneoId
                                  && ejt.Activo);
                if (yaEnTorneo)
                {
                    ErrorMessage = "El jugador ya pertenece a un equipo en este torneo.";
                    return await OnGetAsync(null);
                }

                _context.EquiposJugadoresTorneos.Add(new EquipoJugadorTorneo
                {
                    JugadorId = solicitud.JugadorId,
                    EquipoId = equipoId,
                    TorneoId = solicitud.TorneoId.Value,
                    NumeroCamiseta = numeroCamiseta,
                    FechaInscripcion = DateTime.UtcNow,
                    Activo = true
                });
            }

            solicitud.Estado = EstadosSolicitud.Aceptada;
            solicitud.FechaRespuesta = DateTime.UtcNow;
            solicitud.EntrenadorResponsableId = entrenador.Id;

            await _context.SaveChangesAsync();
            return RedirectToPage(new { message = "Solicitud aceptada. El jugador ha sido agregado a la plantilla." });
        }

        public async Task<IActionResult> OnPostRechazarAsync(int id, string? respuesta)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var result = await ObtenerEntrenadorEquipoAsync();
            if (result == null)
                return RedirectToPage("/Account/Login");

            var (entrenador, equipoId) = result.Value;

            var solicitud = await _context.SolicitudesJugadorEquipo
                .FirstOrDefaultAsync(s => s.Id == id && s.EquipoId == equipoId
                                       && s.Estado == EstadosSolicitud.Pendiente);

            if (solicitud == null)
                return RedirectToPage(new { message = "Solicitud no encontrada." });

            solicitud.Estado = EstadosSolicitud.Rechazada;
            solicitud.FechaRespuesta = DateTime.UtcNow;
            solicitud.EntrenadorResponsableId = entrenador.Id;
            solicitud.RespuestaEntrenador = respuesta;

            await _context.SaveChangesAsync();
            return RedirectToPage(new { message = "Solicitud rechazada." });
        }
    }

    public class SolicitudRecibidaViewModel
    {
        public int Id { get; set; }
        public string NombreJugador { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public string? Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        // Extra player info
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Posicion { get; set; }
        public int? Edad { get; set; }
    }

    public class InvitacionEnviadaViewModel
    {
        public int Id { get; set; }
        public string NombreJugador { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
