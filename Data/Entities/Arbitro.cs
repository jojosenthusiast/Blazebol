using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Arbitro
    {
        public int Id { get; set; }
        public Guid PersonaId { get; set; }
        public string NumeroLicencia { get; set; } = string.Empty;
        public string? CategoriasArbitra { get; set; } // JSON array
        public string EstadoAprobacion { get; set; } = "Pendiente";
        public DateTime? FechaAprobacion { get; set; }

        // Navegación
        public Persona Persona { get; set; } = null!;
    }

    public static class EstadosAprobacionArbitro
    {
        public const string Pendiente = "Pendiente";
        public const string Aprobado = "Aprobado";
        public const string Suspendido = "Suspendido";
        public const string Rechazado = "Rechazado";
    }
}