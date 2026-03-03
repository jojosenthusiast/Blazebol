using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class PreferenciaEspectador
    {
        public int Id { get; set; }
        public Guid UsuarioId { get; set; }
        public int? EquipoFavoritoId { get; set; }
        public bool NotificacionesEmail { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public Equipo? EquipoFavorito { get; set; }
    }
}