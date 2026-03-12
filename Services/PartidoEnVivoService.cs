using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public class PartidoEnVivoService : IPartidoEnVivoService
    {
        private readonly ApplicationDbContext _context;

        public PartidoEnVivoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IniciarPartidoAsync(int partidoId, int arbitroId, INotificacionService notif)
        {
            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == partidoId);

            if (partido == null || partido.ArbitroId != arbitroId)
                return false;

            if (partido.Estado != EstadosPartido.Programado)
                return false;

            partido.Estado = EstadosPartido.EnCurso;
            partido.FechaInicioReal = DateTime.UtcNow;
            partido.MinutoActual = 0;
            partido.FechaActualizacion = DateTime.UtcNow;
            partido.GolesLocal = 0;
            partido.GolesVisitante = 0;

            _context.HistorialesPartido.Add(new HistorialPartido
            {
                PartidoId = partidoId,
                TipoCambio = TiposCambioPartido.InicioPartido,
                EstadoAnterior = EstadosPartido.Programado,
                EstadoNuevo = EstadosPartido.EnCurso,
                Razon = "Partido iniciado por el árbitro",
                FechaCambio = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            string titulo = $"Partido iniciado: {partido.EquipoLocal.Nombre} vs {partido.EquipoVisitante.Nombre}";
            string msg = $"El partido ha comenzado. ¡En vivo ahora!";
            await notif.NotificarPartidoAsync(partidoId, titulo, msg, TiposNotificacion.Partido);

            return true;
        }

        public async Task<bool> RegistrarIncidenciaAsync(int partidoId, string tipo, int equipoId, int? ejtJugadorId, int? minuto, string? desc, bool esAutogol, bool esPenal, int? ejtJugadorSalidaId = null)
        {
            var partido = await _context.Partidos.FindAsync(partidoId);
            if (partido == null || partido.Estado != EstadosPartido.EnCurso)
                return false;

            var incidencia = new IncidenciaPartido
            {
                PartidoId = partidoId,
                Tipo = tipo,
                EquipoId = equipoId,
                EjtJugadorId = ejtJugadorId,
                EjtJugadorSalidaId = ejtJugadorSalidaId,
                Minuto = minuto,
                Descripcion = desc,
                EsAutogol = esAutogol,
                EsPenal = esPenal,
                FechaCreacion = DateTime.UtcNow
            };

            _context.IncidenciasPartido.Add(incidencia);

            // Update minute
            if (minuto.HasValue)
                partido.MinutoActual = minuto.Value;

            await _context.SaveChangesAsync();

            // Recalculate goals if gol
            if (tipo == TiposIncidencia.Gol)
                await RecalcularGolesAsync(partidoId);

            return true;
        }

        public async Task RecalcularGolesAsync(int partidoId)
        {
            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == partidoId);

            if (partido == null) return;

            var goles = await _context.IncidenciasPartido
                .Where(i => i.PartidoId == partidoId && i.Tipo == TiposIncidencia.Gol)
                .ToListAsync();

            int golesLocal = 0, golesVisitante = 0;
            foreach (var g in goles)
            {
                bool esLocal = g.EquipoId == partido.EquipoLocalId;
                if (g.EsAutogol)
                {
                    // Autogol: cuenta para el equipo contrario
                    if (esLocal) golesVisitante++;
                    else golesLocal++;
                }
                else
                {
                    if (esLocal) golesLocal++;
                    else golesVisitante++;
                }
            }

            partido.GolesLocal = golesLocal;
            partido.GolesVisitante = golesVisitante;
            partido.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> FinalizarPartidoAsync(int partidoId, int arbitroId, INotificacionService notif)
        {
            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == partidoId);

            if (partido == null || partido.ArbitroId != arbitroId)
                return false;

            if (partido.Estado != EstadosPartido.EnCurso)
                return false;

            partido.Estado = EstadosPartido.Finalizado;
            partido.FechaActualizacion = DateTime.UtcNow;

            _context.HistorialesPartido.Add(new HistorialPartido
            {
                PartidoId = partidoId,
                TipoCambio = TiposCambioPartido.FinPartido,
                EstadoAnterior = EstadosPartido.EnCurso,
                EstadoNuevo = EstadosPartido.Finalizado,
                Razon = "Partido finalizado por el árbitro",
                FechaCambio = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            string titulo = $"Partido finalizado: {partido.EquipoLocal.Nombre} vs {partido.EquipoVisitante.Nombre}";
            string msg = $"Resultado final: {partido.GolesLocal ?? 0} - {partido.GolesVisitante ?? 0}";
            await notif.NotificarPartidoAsync(partidoId, titulo, msg, TiposNotificacion.Partido);

            return true;
        }

        public async Task<List<IncidenciaPartido>> GetIncidenciasAsync(int partidoId)
        {
            return await _context.IncidenciasPartido
                .Include(i => i.Equipo)
                .Include(i => i.EjtJugador).ThenInclude(ejt => ejt!.Jugador).ThenInclude(j => j.Persona)
                .Include(i => i.EjtJugadorSalida).ThenInclude(ejt => ejt!.Jugador).ThenInclude(j => j.Persona)
                .Where(i => i.PartidoId == partidoId)
                .OrderBy(i => i.Minuto)
                .ThenBy(i => i.FechaCreacion)
                .ToListAsync();
        }

        public async Task<PartidoEnVivoVM> GetEstadoEnVivoAsync(int partidoId)
        {
            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == partidoId);

            if (partido == null)
                return new PartidoEnVivoVM { PartidoId = partidoId };

            var incidencias = await GetIncidenciasAsync(partidoId);

            // Build convocados lists
            var convocadosLocal = await GetConvocadosConEstadoAsync(partidoId, partido.EquipoLocalId, incidencias);
            var convocadosVisitante = await GetConvocadosConEstadoAsync(partidoId, partido.EquipoVisitanteId, incidencias);

            var vm = new PartidoEnVivoVM
            {
                PartidoId = partidoId,
                EquipoLocalId = partido.EquipoLocalId,
                EquipoVisitanteId = partido.EquipoVisitanteId,
                NombreLocal = partido.EquipoLocal.Nombre,
                NombreVisitante = partido.EquipoVisitante.Nombre,
                LogoLocal = partido.EquipoLocal.Logo,
                LogoVisitante = partido.EquipoVisitante.Logo,
                AbrevLocal = partido.EquipoLocal.Abreviacion,
                AbrevVisitante = partido.EquipoVisitante.Abreviacion,
                ColorLocal = partido.EquipoLocal.ColorPrimario,
                ColorVisitante = partido.EquipoVisitante.ColorPrimario,
                GolesLocal = partido.GolesLocal ?? 0,
                GolesVisitante = partido.GolesVisitante ?? 0,
                Estado = partido.Estado,
                MinutoActual = partido.MinutoActual,
                FechaInicioReal = partido.FechaInicioReal,
                JugadoresLocal = convocadosLocal,
                JugadoresVisitante = convocadosVisitante,
                Incidencias = incidencias.Select(i => new IncidenciaVM
                {
                    Id = i.Id,
                    Tipo = i.Tipo,
                    Minuto = i.Minuto,
                    NombreJugador = i.EjtJugador != null
                        ? $"{i.EjtJugador.Jugador.Persona.Nombre} {i.EjtJugador.Jugador.Persona.ApellidoPaterno}"
                        : null,
                    NombreJugadorSalida = i.EjtJugadorSalida != null
                        ? $"{i.EjtJugadorSalida.Jugador.Persona.Nombre} {i.EjtJugadorSalida.Jugador.Persona.ApellidoPaterno}"
                        : null,
                    EsAutogol = i.EsAutogol,
                    EsPenal = i.EsPenal,
                    NombreEquipo = i.Equipo.Nombre,
                    Descripcion = i.Descripcion,
                    FechaCreacion = i.FechaCreacion
                }).ToList()
            };

            return vm;
        }

        private async Task<List<ConvocadoVM>> GetConvocadosConEstadoAsync(int partidoId, int equipoId, List<IncidenciaPartido> incidencias)
        {
            var convocatoria = await _context.Convocatorias
                .Include(c => c.ConvocatoriaJugadores)
                    .ThenInclude(cj => cj.EquipoJugadorTorneo)
                        .ThenInclude(ejt => ejt.Jugador)
                            .ThenInclude(j => j.Persona)
                .FirstOrDefaultAsync(c => c.PartidoId == partidoId && c.EquipoId == equipoId);

            if (convocatoria == null)
                return new List<ConvocadoVM>();

            var result = new List<ConvocadoVM>();

            foreach (var cj in convocatoria.ConvocatoriaJugadores.OrderBy(c => c.EquipoJugadorTorneo.NumeroCamiseta))
            {
                int ejtId = cj.EquipoJugadorTorneoId;

                int amarillas = incidencias.Count(i =>
                    i.EjtJugadorId == ejtId && i.Tipo == TiposIncidencia.TarjetaAmarilla);

                bool tarjetaRoja = incidencias.Any(i =>
                    i.EjtJugadorId == ejtId &&
                    (i.Tipo == TiposIncidencia.TarjetaRoja || i.Tipo == TiposIncidencia.Expulsion));

                bool salio = incidencias.Any(i =>
                    i.EjtJugadorSalidaId == ejtId && i.Tipo == TiposIncidencia.Cambio);

                bool entro = incidencias.Any(i =>
                    i.EjtJugadorId == ejtId && i.Tipo == TiposIncidencia.Cambio);

                bool estaEnCampo;
                if (cj.Rol == RolesConvocatoria.Titular)
                    estaEnCampo = !tarjetaRoja && !salio;
                else
                    estaEnCampo = entro && !tarjetaRoja && !salio;

                result.Add(new ConvocadoVM
                {
                    EjtId = ejtId,
                    NombreCompleto = $"{cj.EquipoJugadorTorneo.Jugador.Persona.Nombre} {cj.EquipoJugadorTorneo.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = cj.EquipoJugadorTorneo.NumeroCamiseta,
                    PosicionAsignada = cj.PosicionAsignada ?? cj.EquipoJugadorTorneo.Jugador.PosicionPreferida,
                    Rol = cj.Rol,
                    TarjetasAmarillas = amarillas,
                    TarjetaRoja = tarjetaRoja,
                    EstaEnCampo = estaEnCampo
                });
            }

            return result;
        }
    }
}
