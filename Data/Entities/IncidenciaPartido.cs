using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class IncidenciaPartido
    {
        public int Id { get; set; }
        public int PartidoId { get; set; }
        public int? ReportePartidoId { get; set; }
        public int EquipoId { get; set; }
        /// <summary>FK a EquipoJugadorTorneo.Id — jugador que ejecuta la acción</summary>
        public int? EjtJugadorId { get; set; }
        /// <summary>FK a EquipoJugadorTorneo.Id — solo para Cambio: quien sale del campo</summary>
        public int? EjtJugadorSalidaId { get; set; }
        /// <summary>Gol | TarjetaAmarilla | TarjetaRoja | Expulsion | Lesion | Cambio | Suspension | Retraso</summary>
        public string Tipo { get; set; } = string.Empty;
        public int? Minuto { get; set; }
        public string? Descripcion { get; set; }
        public bool EsAutogol { get; set; } = false;
        public bool EsPenal { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public Partido Partido { get; set; } = null!;
        public ReportePartido? ReportePartido { get; set; }
        public Equipo Equipo { get; set; } = null!;
        public EquipoJugadorTorneo? EjtJugador { get; set; }
        public EquipoJugadorTorneo? EjtJugadorSalida { get; set; }
    }

    public static class TiposIncidencia
    {
        public const string Gol             = "Gol";
        public const string TarjetaAmarilla = "TarjetaAmarilla";
        public const string TarjetaRoja     = "TarjetaRoja";
        public const string Expulsion       = "Expulsion";
        public const string Lesion          = "Lesion";
        public const string Cambio          = "Cambio";
        public const string Suspension      = "Suspension";
        public const string Retraso         = "Retraso";
    }
}
