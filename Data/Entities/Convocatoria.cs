using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Convocatoria
    {
        public int Id { get; set; }
        public int PartidoId { get; set; }
        public int EquipoId { get; set; }
        public int EntrenadorId { get; set; }
        /// <summary>Borrador | Publicada</summary>
        public string Estado { get; set; } = EstadosConvocatoria.Borrador;
        public DateTime? FechaPublicacion { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public Partido Partido { get; set; } = null!;
        public Equipo Equipo { get; set; } = null!;
        public Entrenador Entrenador { get; set; } = null!;
        public ICollection<ConvocatoriaJugador> ConvocatoriaJugadores { get; set; } = new List<ConvocatoriaJugador>();
    }

    public static class EstadosConvocatoria
    {
        public const string Borrador = "Borrador";
        public const string Publicada = "Publicada";
    }
}
