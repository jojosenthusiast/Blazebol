using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class NotificacionInterna
    {
        public int Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        /// <summary>Partido | Convocatoria | Solicitud | Sistema</summary>
        public string Tipo { get; set; } = "Sistema";
        public int? ReferenciaId { get; set; }   // PartidoId, ConvocatoriaId, etc.
        public string? UrlAccion { get; set; }
        public bool Leida { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaLectura { get; set; }

        // Navegación
        public Usuario Usuario { get; set; } = null!;
    }

    public static class TiposNotificacion
    {
        public const string Partido = "Partido";
        public const string Convocatoria = "Convocatoria";
        public const string Solicitud = "Solicitud";
        public const string Sistema = "Sistema";
    }
}
