using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly ApplicationDbContext _context;

        public NotificacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CrearAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Sistema", int? referenciaId = null, string? urlAccion = null)
        {
            var notif = new NotificacionInterna
            {
                UsuarioId = usuarioId,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                ReferenciaId = referenciaId,
                UrlAccion = urlAccion,
                Leida = false,
                FechaCreacion = DateTime.UtcNow
            };
            _context.NotificacionesInternas.Add(notif);
            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarNoLeidasAsync(Guid usuarioId)
        {
            return await _context.NotificacionesInternas
                .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
        }

        public async Task<List<NotificacionInterna>> GetRecientesAsync(Guid usuarioId, int top = 10)
        {
            return await _context.NotificacionesInternas
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(top)
                .ToListAsync();
        }

        public async Task MarcarLeidaAsync(int notificacionId, Guid usuarioId)
        {
            var notif = await _context.NotificacionesInternas
                .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);
            if (notif != null && !notif.Leida)
            {
                notif.Leida = true;
                notif.FechaLectura = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarcarTodasLeidasAsync(Guid usuarioId)
        {
            var pendientes = await _context.NotificacionesInternas
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ToListAsync();
            foreach (var n in pendientes)
            {
                n.Leida = true;
                n.FechaLectura = DateTime.UtcNow;
            }
            if (pendientes.Any())
                await _context.SaveChangesAsync();
        }

        public async Task NotificarPartidoAsync(int partidoId, string titulo, string mensaje, string tipo = "Partido")
        {
            var partido = await _context.Partidos
                .Include(p => p.Arbitro).ThenInclude(a => a!.Persona).ThenInclude(p => p.Usuario)
                .Include(p => p.EquipoLocal).ThenInclude(e => e.EquiposEntrenadores)
                    .ThenInclude(ee => ee.Entrenador).ThenInclude(en => en.Persona).ThenInclude(p => p.Usuario)
                .Include(p => p.EquipoVisitante).ThenInclude(e => e.EquiposEntrenadores)
                    .ThenInclude(ee => ee.Entrenador).ThenInclude(en => en.Persona).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == partidoId);

            if (partido == null) return;

            var destinatarios = new HashSet<Guid>();

            // Árbitro
            if (partido.Arbitro?.Persona?.Usuario != null)
                destinatarios.Add(partido.Arbitro.Persona.Usuario.Id);

            // Entrenadores equipo local
            foreach (var ee in partido.EquipoLocal.EquiposEntrenadores.Where(x => x.Activo))
                if (ee.Entrenador?.Persona?.Usuario != null)
                    destinatarios.Add(ee.Entrenador.Persona.Usuario.Id);

            // Entrenadores equipo visitante
            foreach (var ee in partido.EquipoVisitante.EquiposEntrenadores.Where(x => x.Activo))
                if (ee.Entrenador?.Persona?.Usuario != null)
                    destinatarios.Add(ee.Entrenador.Persona.Usuario.Id);

            // Jugadores de ambos equipos
            var jugadoresEjt = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona).ThenInclude(p => p.Usuario)
                .Where(ejt => (ejt.EquipoId == partido.EquipoLocalId || ejt.EquipoId == partido.EquipoVisitanteId)
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .ToListAsync();

            foreach (var ejt in jugadoresEjt)
                if (ejt.Jugador?.Persona?.Usuario != null)
                    destinatarios.Add(ejt.Jugador.Persona.Usuario.Id);

            var urlAccion = $"/Admin/Partidos/Details/{partidoId}";
            foreach (var uid in destinatarios)
            {
                _context.NotificacionesInternas.Add(new NotificacionInterna
                {
                    UsuarioId = uid,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Tipo = tipo,
                    ReferenciaId = partidoId,
                    UrlAccion = urlAccion,
                    Leida = false,
                    FechaCreacion = DateTime.UtcNow
                });
            }

            if (destinatarios.Any())
                await _context.SaveChangesAsync();
        }
    }
}
