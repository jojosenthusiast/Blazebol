using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class ReglaPersonalizadaTorneo
    {
        public int Id { get; set; }
        public int TorneoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Suspension, Restriccion, Validacion, Custom
        public bool Activa { get; set; } = true;
        public bool EsAutomatica { get; set; } = true; // Si se valida automáticamente antes del partido
        public int Orden { get; set; } = 0;
        public string? CondicionJson { get; set; } // JSON con condiciones específicas
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public Torneo Torneo { get; set; } = null!;
    }

    public static class TiposRegla
    {
        public const string Suspension = "Suspension"; // Ej: Jugador con roja no puede jugar
        public const string Restriccion = "Restriccion"; // Ej: Máximo de jugadores extranjeros
        public const string Validacion = "Validacion"; // Ej: Verificar documentación
        public const string Custom = "Custom"; // Reglas personalizadas del torneo
    }
}