using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Entrenador
    {
        public int Id { get; set; }
        public Guid PersonaId { get; set; }
        public string? NumeroLicencia { get; set; }
        public DateTime? FechaExpiracionLicencia { get; set; }
        public string? Especialidad { get; set; }

        // Navegación
        public Persona Persona { get; set; } = null!;
        public ICollection<EquipoEntrenador> EquiposEntrenadores { get; set; } = new List<EquipoEntrenador>();
    }
}