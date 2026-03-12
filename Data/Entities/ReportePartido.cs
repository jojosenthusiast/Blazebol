using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class ReportePartido
    {
        public int Id { get; set; }
        public int PartidoId { get; set; }
        public int ArbitroId { get; set; }
        /// <summary>Borrador | Enviado | Reabierto</summary>
        public string Estado { get; set; } = EstadosReporte.Borrador;
        public string? Observaciones { get; set; }
        public int? MinutosJugados { get; set; }
        public bool HuboProrroga { get; set; } = false;
        public bool HuboPenales { get; set; } = false;
        public int? GolesLocalPenales { get; set; }
        public int? GolesVisitantePenales { get; set; }
        public DateTime? FechaEnvio { get; set; }
        public string? MotivoReapertura { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public Partido Partido { get; set; } = null!;
        public Arbitro Arbitro { get; set; } = null!;
        public ICollection<IncidenciaPartido> Incidencias { get; set; } = new List<IncidenciaPartido>();
    }

    public static class EstadosReporte
    {
        public const string Borrador = "Borrador";
        public const string Enviado = "Enviado";
        public const string Reabierto = "Reabierto";
    }
}
