using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class ConvocatoriaJugador
    {
        public int Id { get; set; }
        public int ConvocatoriaId { get; set; }
        public int EquipoJugadorTorneoId { get; set; }
        /// <summary>Titular | Suplente | Reserva</summary>
        public string Rol { get; set; } = RolesConvocatoria.Titular;
        public string? PosicionAsignada { get; set; }
        /// <summary>Pendiente | Confirmo | NoDisponible</summary>
        public string EstadoConfirmacion { get; set; } = EstadosConfirmacion.Pendiente;
        public DateTime? FechaConfirmacion { get; set; }
        public string? NotaConfirmacion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public Convocatoria Convocatoria { get; set; } = null!;
        public EquipoJugadorTorneo EquipoJugadorTorneo { get; set; } = null!;
    }

    public static class RolesConvocatoria
    {
        public const string Titular = "Titular";
        public const string Suplente = "Suplente";
        public const string Reserva = "Reserva";
    }

    public static class EstadosConfirmacion
    {
        public const string Pendiente = "Pendiente";
        public const string Confirmo = "Confirmo";
        public const string NoDisponible = "NoDisponible";
    }
}
