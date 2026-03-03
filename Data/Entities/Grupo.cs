using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Grupo
    {
        public int Id { get; set; }
        public int TorneoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Orden { get; set; }

        // Navegación
        public Torneo Torneo { get; set; } = null!;
        public ICollection<EquipoTorneo> EquiposTorneos { get; set; } = new List<EquipoTorneo>();
    }
}