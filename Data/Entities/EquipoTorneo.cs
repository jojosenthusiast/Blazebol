using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class EquipoTorneo
    {
        public int Id { get; set; }
        public int EquipoId { get; set; }
        public int TorneoId { get; set; }
        public int? GrupoId { get; set; }
        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Activo";

        // Navegación
        public Equipo Equipo { get; set; } = null!;
        public Torneo Torneo { get; set; } = null!;
        public Grupo? Grupo { get; set; }
    }
}