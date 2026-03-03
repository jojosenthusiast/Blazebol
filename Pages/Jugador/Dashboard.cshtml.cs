using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Jugador
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreCompleto { get; set; } = string.Empty;
        public string? EquipoActual { get; set; }
        public string? TorneoActual { get; set; }
        public string? PosicionActual { get; set; }
        public int? NumeroCamisetaActual { get; set; }
        public int PartidosJugados { get; set; }
        public int Goles { get; set; }
        public int Asistencias { get; set; }
        public int TarjetasAmarillas { get; set; }
        public int TarjetasRojas { get; set; }
        public List<string> ProximosPartidos { get; set; } = new();

        // HU-2: estado de inscripción
        public bool TieneEquipoEnTorneoActivo { get; set; }
        public bool TieneSolicitudPendiente { get; set; }
        public int SolicitudesPendientesCount { get; set; }
        public int InvitacionesPendientesCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return RedirectToPage("/Account/Login");

            if (!HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Error", new { statusCode = 403 });

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Jugador == null)
                return RedirectToPage("/Error");

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";
            var jugadorId = persona.Jugador.Id;
            PosicionActual = persona.Jugador.PosicionPreferida;

            // Inscripción en torneo activo (equipo activo en torneo activo)
            var ejt = await _context.EquiposJugadoresTorneos
                .Include(e => e.Equipo)
                .Include(e => e.Torneo)
                .Where(e => e.JugadorId == jugadorId && e.Activo
                         && e.Torneo.Estado == EstadosTorneo.Activo)
                .OrderByDescending(e => e.FechaInscripcion)
                .FirstOrDefaultAsync();

            TieneEquipoEnTorneoActivo = ejt != null;

            if (ejt != null)
            {
                EquipoActual = ejt.Equipo.Nombre;
                TorneoActual = ejt.Torneo.Nombre;
                NumeroCamisetaActual = ejt.NumeroCamiseta;
            }
            else
            {
                // Fallback: cualquier equipo activo aunque no sea en torneo activo
                var ejtCualquiera = await _context.EquiposJugadoresTorneos
                    .Include(e => e.Equipo)
                    .Include(e => e.Torneo)
                    .Where(e => e.JugadorId == jugadorId && e.Activo)
                    .OrderByDescending(e => e.FechaInscripcion)
                    .FirstOrDefaultAsync();

                if (ejtCualquiera != null)
                {
                    EquipoActual = ejtCualquiera.Equipo.Nombre;
                    TorneoActual = ejtCualquiera.Torneo.Nombre;
                }
            }

            // Solicitudes: enviadas pendientes + invitaciones pendientes
            SolicitudesPendientesCount = await _context.SolicitudesJugadorEquipo
                .CountAsync(s => s.JugadorId == jugadorId
                             && !s.EsInvitacion
                             && s.Estado == "Pendiente");

            InvitacionesPendientesCount = await _context.SolicitudesJugadorEquipo
                .CountAsync(s => s.JugadorId == jugadorId
                             && s.EsInvitacion
                             && s.Estado == "Pendiente");

            TieneSolicitudPendiente = SolicitudesPendientesCount > 0 || InvitacionesPendientesCount > 0;

            // Estadísticas — placeholder (entidad de eventos no implementada aún)
            PartidosJugados = 0;
            Goles = 0;
            Asistencias = 0;
            TarjetasAmarillas = 0;
            TarjetasRojas = 0;
            ProximosPartidos = new();

            return Page();
        }
    }
}
