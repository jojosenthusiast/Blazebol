using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public interface IPdfService
    {
        byte[] GenerateUsuariosReport(List<Usuario> usuarios);
        byte[] GenerateTorneosReport(List<Torneo> torneos);
        byte[] GenerateJugadoresReport(List<Jugador> jugadores);
        byte[] GenerarReglamentoPdf(Torneo torneo, ConfiguracionTorneo? config);
    }
}