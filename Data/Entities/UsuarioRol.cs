using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class UsuarioRol
    {
        public int Id { get; set; }
        public Guid UsuarioId { get; set; }
        public int RolId { get; set; }
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public Rol Rol { get; set; } = null!;
    }
}