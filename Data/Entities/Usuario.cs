using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmado { get; set; } = false;
        public string PasswordHash { get; set; } = string.Empty;
        public string? NombreUsuario { get; set; }
        public string? TelefonoNumero { get; set; }
        public bool TelefonoConfirmado { get; set; } = false;
        public bool AutenticacionDosFactores { get; set; } = false;
        public DateTime? FechaBloqueoFin { get; set; }
        public bool BloqueoHabilitado { get; set; } = true;
        public int ContadorAccesosFallidos { get; set; } = 0;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; } = true;

        // Relaciones
        public Persona? Persona { get; set; }
        public ICollection<UsuarioRol> UsuariosRoles { get; set; } = new List<UsuarioRol>();
    }
}