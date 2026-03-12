using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class HistorialPartido
    {
        public int Id { get; set; }
        public int PartidoId { get; set; }
        public Guid? CambiadoPorUsuarioId { get; set; }
        /// <summary>Reprogramacion | CambioSede | CambioArbitro | Cancelacion | InicioPartido | FinPartido</summary>
        public string TipoCambio { get; set; } = string.Empty;
        public DateTime? FechaAnterior { get; set; }
        public DateTime? FechaNueva { get; set; }
        public string? SedeAnterior { get; set; }
        public string? SedeNueva { get; set; }
        public string? EstadoAnterior { get; set; }
        public string? EstadoNuevo { get; set; }
        public string? Razon { get; set; }
        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        // Navegación
        public Partido Partido { get; set; } = null!;
        public Usuario? CambiadoPor { get; set; }
    }

    public static class TiposCambioPartido
    {
        public const string Reprogramacion = "Reprogramacion";
        public const string CambioSede = "CambioSede";
        public const string CambioArbitro = "CambioArbitro";
        public const string Cancelacion = "Cancelacion";
        public const string InicioPartido = "InicioPartido";
        public const string FinPartido = "FinPartido";
    }
}
