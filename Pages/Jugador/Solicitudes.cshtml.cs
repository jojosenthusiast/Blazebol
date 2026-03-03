using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Jugador
{
    public class SolicitudesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SolicitudesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        // Invitaciones que el coach envió al jugador (EsInvitacion=true, Pendiente)
        public List<SolicitudJugadorViewModel> InvitacionesRecibidas { get; set; } = new();

        // Solicitudes que el jugador envió a equipos (EsInvitacion=false)
        public List<SolicitudJugadorViewModel> MisSolicitudes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Jugador == null)
                return RedirectToPage("/Error");

            var jugadorId = persona.Jugador.Id;

            var todasSolicitudes = await _context.SolicitudesJugadorEquipo
                .Include(s => s.Equipo)
                    .ThenInclude(eq => eq.EquiposEntrenadores.Where(ee => ee.Activo && ee.EsPropietario))
                    .ThenInclude(ee => ee.Entrenador).ThenInclude(en => en.Persona)
                .Include(s => s.Torneo)
                .Where(s => s.JugadorId == jugadorId)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            InvitacionesRecibidas = todasSolicitudes
                .Where(s => s.EsInvitacion && s.Estado == EstadosSolicitud.Pendiente)
                .Select(s => MapToViewModel(s))
                .ToList();

            MisSolicitudes = todasSolicitudes
                .Where(s => !s.EsInvitacion)
                .Select(s => MapToViewModel(s))
                .ToList();

            return Page();
        }

        private static SolicitudJugadorViewModel MapToViewModel(SolicitudJugadorEquipo s)
        {
            var entrenador = s.Equipo?.EquiposEntrenadores?.FirstOrDefault();
            string? nombreEntrenador = entrenador != null
                ? $"{entrenador.Entrenador.Persona.Nombre} {entrenador.Entrenador.Persona.ApellidoPaterno}"
                : null;

            return new SolicitudJugadorViewModel
            {
                Id = s.Id,
                NombreEquipo = s.Equipo?.Nombre ?? "—",
                Abreviacion = s.Equipo?.Abreviacion,
                Logo = s.Equipo?.Logo,
                ColorPrimario = s.Equipo?.ColorPrimario,
                Sede = s.Equipo?.Sede,
                NombreEntrenador = nombreEntrenador,
                NombreTorneo = s.Torneo?.Nombre ?? "—",
                Estado = s.Estado,
                FechaSolicitud = s.FechaSolicitud,
                Mensaje = s.MensajeJugador
            };
        }

        public async Task<IActionResult> OnPostAceptarInvitacionAsync(int id, int numeroCamiseta)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Jugador == null)
                return RedirectToPage("/Error");

            var jugadorId = persona.Jugador.Id;

            var solicitud = await _context.SolicitudesJugadorEquipo
                .Include(s => s.Equipo)
                .FirstOrDefaultAsync(s => s.Id == id && s.JugadorId == jugadorId && s.EsInvitacion);

            if (solicitud == null)
            {
                ErrorMessage = "Invitación no encontrada.";
                return await OnGetAsync(null);
            }

            if (solicitud.Estado != EstadosSolicitud.Pendiente)
            {
                ErrorMessage = "Esta invitación ya fue procesada.";
                return await OnGetAsync(null);
            }

            // Check jugador not already in torneo
            var yaEnTorneo = await _context.EquiposJugadoresTorneos
                .AnyAsync(ejt => ejt.JugadorId == jugadorId
                              && ejt.TorneoId == solicitud.TorneoId
                              && ejt.Activo);

            if (yaEnTorneo)
            {
                ErrorMessage = "Ya eres miembro activo de un equipo en ese torneo.";
                solicitud.Estado = EstadosSolicitud.Rechazada;
                await _context.SaveChangesAsync();
                return await OnGetAsync(null);
            }

            // Check jersey number not taken in that equipo+torneo
            var numeroCamisetaTomado = await _context.EquiposJugadoresTorneos
                .AnyAsync(ejt => ejt.EquipoId == solicitud.EquipoId
                              && ejt.TorneoId == solicitud.TorneoId
                              && ejt.NumeroCamiseta == numeroCamiseta
                              && ejt.Activo);

            if (numeroCamisetaTomado)
            {
                ErrorMessage = $"El número de camiseta {numeroCamiseta} ya está en uso en ese equipo para este torneo.";
                return await OnGetAsync(null);
            }

            var inscripcion = new EquipoJugadorTorneo
            {
                JugadorId = jugadorId,
                EquipoId = solicitud.EquipoId,
                TorneoId = solicitud.TorneoId ?? 0,
                NumeroCamiseta = numeroCamiseta,
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };

            _context.EquiposJugadoresTorneos.Add(inscripcion);
            solicitud.Estado = EstadosSolicitud.Aceptada;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { message = $"Te uniste a {solicitud.Equipo?.Nombre ?? "el equipo"} con la camiseta #{numeroCamiseta}." });
        }

        public async Task<IActionResult> OnPostRechazarInvitacionAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Jugador == null)
                return RedirectToPage("/Error");

            var jugadorId = persona.Jugador.Id;

            var solicitud = await _context.SolicitudesJugadorEquipo
                .FirstOrDefaultAsync(s => s.Id == id && s.JugadorId == jugadorId && s.EsInvitacion);

            if (solicitud == null || solicitud.Estado != EstadosSolicitud.Pendiente)
            {
                ErrorMessage = "Invitación no encontrada o ya procesada.";
                return await OnGetAsync(null);
            }

            solicitud.Estado = EstadosSolicitud.Rechazada;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { message = "Invitación rechazada." });
        }
    }

    public class SolicitudJugadorViewModel
    {
        public int Id { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? ColorPrimario { get; set; }
        public string? Sede { get; set; }
        public string? NombreEntrenador { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public string? Mensaje { get; set; }
    }
}
