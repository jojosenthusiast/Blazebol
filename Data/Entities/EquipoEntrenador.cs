using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class EquipoEntrenador
    {
        public int Id { get; set; }
        public int EntrenadorId { get; set; }
        public int EquipoId { get; set; }
        public string Rol { get; set; } = "Principal";
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool EsPropietario { get; set; } = false;
        public bool Activo { get; set; } = true;

        // Navegación
        public Entrenador Entrenador { get; set; } = null!;
        public Equipo Equipo { get; set; } = null!;
    }

    public static class RolesEntrenador
    {
        public const string Principal = "Principal";
        public const string Asistente = "Asistente";
        public const string PreparadorFisico = "PreparadorFisico";
        public const string MedicoDeportivo = "MedicoDeportivo";
    }
}