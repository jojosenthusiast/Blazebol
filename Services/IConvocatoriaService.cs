using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public interface IConvocatoriaService
    {
        Task<Convocatoria?> GetByPartidoEquipoAsync(int partidoId, int equipoId);
        Task<Convocatoria> CrearOrActualizarAsync(int partidoId, int equipoId, int entrenadorId, string observaciones);
        Task<bool> AgregarJugadorAsync(int convocatoriaId, int ejtId, string rol, string? posicion);
        Task<bool> QuitarJugadorAsync(int convocatoriaId, int ejtId);
        Task<bool> PublicarAsync(int convocatoriaId, INotificacionService notificaciones);
        Task<bool> ConfirmarAsistenciaAsync(int convocatoriaId, int ejtId, string estado, string? nota);
        Task<List<ConvocatoriaJugador>> GetJugadoresAsync(int convocatoriaId);
        int ContarTitulares(IEnumerable<ConvocatoriaJugador> jugadores);
    }
}
