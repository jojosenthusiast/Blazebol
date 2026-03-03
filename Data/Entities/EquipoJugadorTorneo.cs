using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class EquipoJugadorTorneo
    {
        public int Id { get; set; }
        public int JugadorId { get; set; }
        public int EquipoId { get; set; }
        public int TorneoId { get; set; }
        public int NumeroCamiseta { get; set; }
        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public Jugador Jugador { get; set; } = null!;
        public Equipo Equipo { get; set; } = null!;
        public Torneo Torneo { get; set; } = null!;
    }
}