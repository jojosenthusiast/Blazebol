namespace ControlTorneosFutbol.Data.Entities
{
    public class ConfiguracionTorneo
    {
        public int Id { get; set; }
        public int TorneoId { get; set; }
        public int PuntosPorVictoria { get; set; } = 3;
        public int PuntosPorEmpate { get; set; } = 1;
        public int PuntosPorDerrota { get; set; } = 0;
        public string CriterioDesempate1 { get; set; } = "Puntos";
        public string? CriterioDesempate2 { get; set; } = "DiferenciaGoles";
        public string? CriterioDesempate3 { get; set; } = "GolesFavor";
        public string? CriterioDesempate4 { get; set; } = "ResultadoDirecto";
        public int TarjetasAmarillasParaSuspension { get; set; } = 3;
        public int PartidosSuspensionTarjetaRoja { get; set; } = 2;
        public int? MaximoJugadoresConvocados { get; set; } = 18;
        public int MinimoJugadoresTitulares { get; set; } = 11;

        // Fixture automático
        public bool IdaYVuelta { get; set; } = false;
        public bool CampoNeutral { get; set; } = false;
        public bool EquilibrioLocalVisitante { get; set; } = true;
        public int? NumeroGrupos { get; set; }
        public int? EquiposPorGrupo { get; set; }
        public int? ClasificanPorGrupo { get; set; } = 2;

        // Navegación
        public Torneo Torneo { get; set; } = null!;
    }
}