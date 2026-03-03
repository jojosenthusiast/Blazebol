using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Equipo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Abreviacion { get; set; } // 3 letters, e.g. "DSS"
        public string? Logo { get; set; } // relative URL path to shield image
        public string? ColorPrimario { get; set; }
        public string? ColorSecundario { get; set; }
        public string? Sede { get; set; }
        public DateTime? FechaFundacion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        // Navegación
        public ICollection<EquipoTorneo> EquiposTorneos { get; set; } = new List<EquipoTorneo>();
        public ICollection<EquipoEntrenador> EquiposEntrenadores { get; set; } = new List<EquipoEntrenador>();
    }
}