using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Jugador
{
    public class BuscarEquiposModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BuscarEquiposModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string? BuscarTerm { get; set; }
        public int? TorneoFiltro { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public List<EquipoResultadoViewModel> Equipos { get; set; } = new();
        public List<TorneoOpcionViewModel> TorneosActivos { get; set; } = new();

        // Jugador profile data for compatibility filtering
        private string? _generoJugador;
        private DateTime? _fechaNacimientoJugador;
        private int? _jugadorId;

        private async Task<(int? jugadorId, string? genero, DateTime? fechaNacimiento)> ObtenerDatosJugadorAsync()
        {
            if (_jugadorId.HasValue) return (_jugadorId, _generoJugador, _fechaNacimientoJugador);
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return (null, null, null);

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            _jugadorId = persona?.Jugador?.Id;
            _generoJugador = persona?.Genero;
            _fechaNacimientoJugador = persona?.FechaNacimiento;
            return (_jugadorId, _generoJugador, _fechaNacimientoJugador);
        }

        private async Task<int?> ObtenerJugadorIdAsync()
        {
            var (id, _, _) = await ObtenerDatosJugadorAsync();
            return id;
        }

        public async Task<IActionResult> OnGetAsync(string? buscar, int? torneoId, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            BuscarTerm = buscar;
            TorneoFiltro = torneoId;

            var (jugadorId, genero, fechaNacimiento) = await ObtenerDatosJugadorAsync();
            if (!jugadorId.HasValue)
            {
                ErrorMessage = "No se encontró el perfil de jugador. Contacta al administrador.";
                return Page();
            }

            // Torneos activos para el filtro
            TorneosActivos = await _context.Torneos
                .Where(t => t.Estado == EstadosTorneo.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new TorneoOpcionViewModel { Id = t.Id, Nombre = t.Nombre })
                .ToListAsync();

            // Construir query de equipos (sin criterio = mostrar todos, máx 40)
            var equiposQuery = _context.Equipos.Where(e => e.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var termLower = buscar.ToLower();
                equiposQuery = equiposQuery.Where(e =>
                    e.Nombre.ToLower().Contains(termLower) ||
                    (e.Abreviacion != null && e.Abreviacion.ToLower().Contains(termLower)));
            }

            if (torneoId.HasValue)
            {
                var equiposEnTorneo = _context.EquiposTorneos
                    .Where(et => et.TorneoId == torneoId.Value)
                    .Select(et => et.EquipoId);
                equiposQuery = equiposQuery.Where(e => equiposEnTorneo.Contains(e.Id));
            }

            var equiposRaw = await equiposQuery
                .OrderBy(e => e.Nombre)
                .Take(80)
                .ToListAsync();

            // Filter by TipoEquipo compatibility
            var equipos = equiposRaw
                .Where(e => TiposEquipo.ValidarJugador(e.TipoEquipo, genero, fechaNacimiento).ok)
                .Take(40)
                .ToList();

            if (!equipos.Any())
            {
                return Page();
            }

            var equipoIds = equipos.Select(e => e.Id).ToList();

            // Solicitudes pendientes del jugador
            var solicitudesPendientes = await _context.SolicitudesJugadorEquipo
                .Where(s => s.JugadorId == jugadorId.Value
                         && s.Estado == EstadosSolicitud.Pendiente
                         && !s.EsInvitacion)
                .Select(s => s.EquipoId)
                .ToListAsync();

            // Equipos donde el jugador ya está activo
            var equiposConMiembro = await _context.EquiposJugadoresTorneos
                .Where(ejt => ejt.JugadorId == jugadorId.Value && ejt.Activo)
                .Select(ejt => ejt.EquipoId)
                .Distinct()
                .ToListAsync();

            // Torneos activos de los equipos encontrados
            var equiposTorneoRaw = await _context.EquiposTorneos
                .Include(et => et.Torneo)
                .Where(et => equipoIds.Contains(et.EquipoId))
                .ToListAsync();

            var equiposTorneos = equiposTorneoRaw
                .Where(et => et.Torneo != null && et.Torneo.Estado == EstadosTorneo.Activo)
                .ToList();

            // Entrenadores principales de cada equipo
            var entrenadoresRaw = await _context.EquiposEntrenadores
                .Include(ee => ee.Entrenador).ThenInclude(en => en.Persona)
                .Where(ee => equipoIds.Contains(ee.EquipoId) && ee.Activo && ee.EsPropietario)
                .ToListAsync();

            Equipos = equipos.Select(e =>
            {
                var torneoNombres = equiposTorneos
                    .Where(et => et.EquipoId == e.Id)
                    .Select(et => et.Torneo.Nombre)
                    .ToList();

                var entrenador = entrenadoresRaw.FirstOrDefault(ee => ee.EquipoId == e.Id);
                string? nombreEntrenador = entrenador != null
                    ? $"{entrenador.Entrenador.Persona.Nombre} {entrenador.Entrenador.Persona.ApellidoPaterno}"
                    : null;

                bool yaMiembro = equiposConMiembro.Contains(e.Id);
                bool solicitudPendiente = solicitudesPendientes.Contains(e.Id);

                return new EquipoResultadoViewModel
                {
                    EquipoId = e.Id,
                    Nombre = e.Nombre,
                    Abreviacion = e.Abreviacion,
                    Logo = e.Logo,
                    ColorPrimario = e.ColorPrimario,
                    Sede = e.Sede ?? "—",
                    NombreEntrenador = nombreEntrenador,
                    TorneosActivos = torneoNombres,
                    TipoEquipo = e.TipoEquipo,
                    YaMiembro = yaMiembro,
                    SolicitudPendiente = solicitudPendiente,
                    PuedeSolicitar = !yaMiembro && !solicitudPendiente
                };
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostSolicitarAsync(int equipoId, int? torneoId, string? mensaje)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            var (jugadorId, genero, fechaNacimiento) = await ObtenerDatosJugadorAsync();
            if (!jugadorId.HasValue)
                return RedirectToPage("/Account/Login");

            var equipo = await _context.Equipos.FindAsync(equipoId);
            if (equipo == null)
            {
                ErrorMessage = "El equipo no existe.";
                return await OnGetAsync(null, torneoId, null);
            }

            // Validate TipoEquipo compatibility
            var (compatible, razon) = TiposEquipo.ValidarJugador(equipo.TipoEquipo, genero, fechaNacimiento);
            if (!compatible)
            {
                ErrorMessage = razon;
                return await OnGetAsync(null, torneoId, null);
            }

            var existePendiente = await _context.SolicitudesJugadorEquipo
                .AnyAsync(s => s.JugadorId == jugadorId.Value
                             && s.EquipoId == equipoId
                             && s.Estado == EstadosSolicitud.Pendiente
                             && !s.EsInvitacion);

            if (existePendiente)
            {
                ErrorMessage = "Ya tienes una solicitud pendiente para ese equipo.";
                return await OnGetAsync(null, torneoId, null);
            }

            var yaMiembro = await _context.EquiposJugadoresTorneos
                .AnyAsync(ejt => ejt.JugadorId == jugadorId.Value
                              && ejt.EquipoId == equipoId
                              && ejt.Activo);

            if (yaMiembro)
            {
                ErrorMessage = "Ya eres miembro activo de ese equipo.";
                return await OnGetAsync(null, torneoId, null);
            }

            // Determinar torneo: usa el filtro si viene, sino el primero activo del equipo
            int? torneoFinal = torneoId;
            if (!torneoFinal.HasValue)
            {
                var et = await _context.EquiposTorneos
                    .Include(x => x.Torneo)
                    .Where(x => x.EquipoId == equipoId && x.Torneo.Estado == EstadosTorneo.Activo)
                    .FirstOrDefaultAsync();
                torneoFinal = et?.TorneoId;
            }

            var solicitud = new SolicitudJugadorEquipo
            {
                JugadorId = jugadorId.Value,
                EquipoId = equipoId,
                TorneoId = torneoFinal,
                Estado = EstadosSolicitud.Pendiente,
                EsInvitacion = false,
                MensajeJugador = mensaje,
                FechaSolicitud = DateTime.UtcNow
            };

            _context.SolicitudesJugadorEquipo.Add(solicitud);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { message = $"Solicitud enviada a {equipo.Nombre}. El entrenador revisará tu petición." });
        }
    }

    public class EquipoResultadoViewModel
    {
        public int EquipoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? ColorPrimario { get; set; }
        public string Sede { get; set; } = string.Empty;
        public string? NombreEntrenador { get; set; }
        public List<string> TorneosActivos { get; set; } = new();
        public string TipoEquipo { get; set; } = "Libre";
        public bool YaMiembro { get; set; }
        public bool SolicitudPendiente { get; set; }
        public bool PuedeSolicitar { get; set; }
    }

    public class TorneoOpcionViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
