using System;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Partido
    {
        public int Id { get; set; }
        public int TorneoId { get; set; }
        public int? GrupoId { get; set; }
        public int EquipoLocalId { get; set; }
        public int EquipoVisitanteId { get; set; }
        public int? ArbitroId { get; set; }
        public DateTime Fecha { get; set; }
        public string? Sede { get; set; }
        public string Estado { get; set; } = EstadosPartido.Programado;
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public string? Fase { get; set; }        // "Grupo A", "Octavos", "Cuartos", "Semifinal", "Final"
        public int? Jornada { get; set; }        // Número de jornada dentro del torneo/grupo
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public Torneo Torneo { get; set; } = null!;
        public Grupo? Grupo { get; set; }
        public Equipo EquipoLocal { get; set; } = null!;
        public Equipo EquipoVisitante { get; set; } = null!;
        public Arbitro? Arbitro { get; set; }
    }

    public static class EstadosPartido
    {
        public const string Programado = "Programado";
        public const string EnCurso = "EnCurso";
        public const string Finalizado = "Finalizado";
        public const string Cancelado = "Cancelado";
    }
}
