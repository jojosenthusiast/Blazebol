using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Pages.Calendario;

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
        public List<ProximoPartidoDashVM> ProximosPartidos { get; set; } = new();

        private static readonly string[] _torneoColors = { "#a3ff12", "#5ac8fa", "#ff9500", "#bf5af2", "#ff2d55", "#34c759", "#ffd60a" };
        private static string GetTorneoColor(int id) => _torneoColors[Math.Abs(id) % _torneoColors.Length];

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

            // Próximos partidos del equipo del jugador
            var ejtActivo = await _context.EquiposJugadoresTorneos
                .Where(e => e.JugadorId == jugadorId && e.Activo)
                .OrderByDescending(e => e.FechaInscripcion)
                .FirstOrDefaultAsync();

            if (ejtActivo != null)
            {
                var miEquipoId = ejtActivo.EquipoId;
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
