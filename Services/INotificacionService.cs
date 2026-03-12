using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public interface INotificacionService
    {
        Task CrearAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Sistema", int? referenciaId = null, string? urlAccion = null);
        Task<int> ContarNoLeidasAsync(Guid usuarioId);
        Task<List<NotificacionInterna>> GetRecientesAsync(Guid usuarioId, int top = 10);
        Task MarcarLeidaAsync(int notificacionId, Guid usuarioId);
        Task MarcarTodasLeidasAsync(Guid usuarioId);
        Task NotificarPartidoAsync(int partidoId, string titulo, string mensaje, string tipo = "Partido");
    }
}
