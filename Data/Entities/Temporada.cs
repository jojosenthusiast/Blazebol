using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Temporada
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // "Apertura 2026"
        public int Anio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; } = true;

        // Navegación
        public ICollection<Torneo> Torneos { get; set; } = new List<Torneo>();
    }
}