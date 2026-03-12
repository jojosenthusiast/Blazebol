using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public class ConvocatoriaService : IConvocatoriaService
    {
        private readonly ApplicationDbContext _context;

        public ConvocatoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Convocatoria?> GetByPartidoEquipoAsync(int partidoId, int equipoId)
        {
            return await _context.Convocatorias
                .Include(c => c.ConvocatoriaJugadores)
                    .ThenInclude(cj => cj.EquipoJugadorTorneo)
                        .ThenInclude(ejt => ejt.Jugador)
                            .ThenInclude(j => j.Persona)
                .FirstOrDefaultAsync(c => c.PartidoId == partidoId && c.EquipoId == equipoId);
        }

        public async Task<Convocatoria> CrearOrActualizarAsync(int partidoId, int equipoId, int entrenadorId, string observaciones)
        {
            var existing = await _context.Convocatorias
                .FirstOrDefaultAsync(c => c.PartidoId == partidoId && c.EquipoId == equipoId);

            if (existing != null)
            {
                existing.Observaciones = observaciones;
                existing.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existing;
            }

            var convocatoria = new Convocatoria
            {
                PartidoId = partidoId,
                EquipoId = equipoId,
                EntrenadorId = entrenadorId,
                Estado = EstadosConvocatoria.Borrador,
                Observaciones = observaciones,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Convocatorias.Add(convocatoria);
            await _context.SaveChangesAsync();
            return convocatoria;
        }

        public async Task<bool> AgregarJugadorAsync(int convocatoriaId, int ejtId, string rol, string? posicion)
        {
            var existing = await _context.ConvocatoriasJugadores
                .FirstOrDefaultAsync(cj => cj.ConvocatoriaId == convocatoriaId && cj.EquipoJugadorTorneoId == ejtId);

            if (existing != null)
            {
                existing.Rol = rol;
                existing.PosicionAsignada = posicion;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.ConvocatoriasJugadores.Add(new ConvocatoriaJugador
            {
                ConvocatoriaId = convocatoriaId,
                EquipoJugadorTorneoId = ejtId,
                Rol = rol,
                PosicionAsignada = posicion,
                EstadoConfirmacion = EstadosConfirmacion.Pendiente,
                FechaCreacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuitarJugadorAsync(int convocatoriaId, int ejtId)
        {
            var cj = await _context.ConvocatoriasJugadores
                .FirstOrDefaultAsync(c => c.ConvocatoriaId == convocatoriaId && c.EquipoJugadorTorneoId == ejtId);

            if (cj == null) return false;

            _context.ConvocatoriasJugadores.Remove(cj);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublicarAsync(int convocatoriaId, INotificacionService notificaciones)
        {
            var convocatoria = await _context.Convocatorias
                .Include(c => c.ConvocatoriaJugadores)
                    .ThenInclude(cj => cj.EquipoJugadorTorneo)
                        .ThenInclude(ejt => ejt.Jugador)
                            .ThenInclude(j => j.Persona)
                                .ThenInclude(p => p.Usuario)
                .Include(c => c.Partido)
                    .ThenInclude(p => p.EquipoLocal)
                .Include(c => c.Partido)
                    .ThenInclude(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(c => c.Id == convocatoriaId);

            if (convocatoria == null) return false;

            convocatoria.Estado = EstadosConvocatoria.Publicada;
            convocatoria.FechaPublicacion = DateTime.UtcNow;
            convocatoria.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notificar a los jugadores convocados
            var partido = convocatoria.Partido;
            string titulo = $"Convocatoria publicada — {partido.EquipoLocal.Nombre} vs {partido.EquipoVisitante.Nombre}";
            string mensaje = $"Fuiste convocado para el partido del {partido.Fecha:dd/MM/yyyy HH:mm}. Confirma tu asistencia.";

            foreach (var cj in convocatoria.ConvocatoriaJugadores)
            {
                var usuarioId = cj.EquipoJugadorTorneo?.Jugador?.Persona?.Usuario?.Id;
                if (usuarioId.HasValue)
                {
                    await notificaciones.CrearAsync(
                        usuarioId.Value,
                        titulo,
                        mensaje,
                        TiposNotificacion.Convocatoria,
                        convocatoria.PartidoId,
                        $"/Jugador/Partidos/Index"
                    );
                }
            }

            return true;
        }

        public async Task<bool> ConfirmarAsistenciaAsync(int convocatoriaId, int ejtId, string estado, string? nota)
        {
            var cj = await _context.ConvocatoriasJugadores
                .FirstOrDefaultAsync(c => c.ConvocatoriaId == convocatoriaId && c.EquipoJugadorTorneoId == ejtId);

            if (cj == null) return false;

            cj.EstadoConfirmacion = estado;
            cj.NotaConfirmacion = nota;
            cj.FechaConfirmacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ConvocatoriaJugador>> GetJugadoresAsync(int convocatoriaId)
        {
            return await _context.ConvocatoriasJugadores
                .Include(cj => cj.EquipoJugadorTorneo)
                    .ThenInclude(ejt => ejt.Jugador)
                        .ThenInclude(j => j.Persona)
                .Where(cj => cj.ConvocatoriaId == convocatoriaId)
                .OrderBy(cj => cj.Rol)
                .ThenBy(cj => cj.EquipoJugadorTorneo.NumeroCamiseta)
                .ToListAsync();
        }

        public int ContarTitulares(IEnumerable<ConvocatoriaJugador> jugadores)
        {
            return jugadores.Count(j => j.Rol == RolesConvocatoria.Titular);
        }
    }
}
