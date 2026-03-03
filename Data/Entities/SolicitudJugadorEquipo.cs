using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class SolicitudJugadorEquipo
    {
        public int Id { get; set; }
        public int JugadorId { get; set; }
        public int EquipoId { get; set; }
        public int? TorneoId { get; set; }
        public string? MensajeJugador { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aceptada, Rechazada
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public DateTime? FechaRespuesta { get; set; }
        public string? RespuestaEntrenador { get; set; }
        public int? EntrenadorResponsableId { get; set; }
        // true = coach invitó al jugador; false = jugador solicitó unirse
        public bool EsInvitacion { get; set; } = false;

        // Navegación
        public Jugador Jugador { get; set; } = null!;
        public Equipo Equipo { get; set; } = null!;
        public Torneo? Torneo { get; set; }
        public Entrenador? EntrenadorResponsable { get; set; }
    }

    public static class EstadosSolicitud
    {
        public const string Pendiente = "Pendiente";
        public const string Aceptada = "Aceptada";
        public const string Rechazada = "Rechazada";
    }
}