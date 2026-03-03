using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Persona
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? IdentificacionCifrada { get; set; } // DUI/Pasaporte cifrado
        public string? Telefono { get; set; }
        public string? FotoPerfil { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public Jugador? Jugador { get; set; }
        public Entrenador? Entrenador { get; set; }
        public Arbitro? Arbitro { get; set; }
    }
}