using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class AuditoriaAccesos
    {
        public int Id { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? Email { get; set; }
        public bool Exitoso { get; set; }
        public string? DireccionIP { get; set; }
        public string? UserAgent { get; set; }
        public DateTime FechaIntento { get; set; } = DateTime.UtcNow;
        public string? MotivoFallo { get; set; }

        // Navegación
        public Usuario? Usuario { get; set; }
    }
}