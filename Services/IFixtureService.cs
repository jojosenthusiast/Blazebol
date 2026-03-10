using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public interface IFixtureService
    {
        /// <summary>Genera fixture round-robin (todos contra todos). Soporta ida y vuelta.</summary>
        Task<List<Partido>> GenerarTodosContraTodos(int torneoId, ConfiguracionTorneo config, List<int> equipoIds);

        /// <summary>Genera fixture de eliminación directa (bracket). Requiere potencia de 2.</summary>
        Task<List<Partido>> GenerarEliminacionDirecta(int torneoId, List<int> equipoIds);

        /// <summary>Genera fase de grupos (Champions). Requiere NumeroGrupos y EquiposPorGrupo configurados.</summary>
        Task<List<Partido>> GenerarGrupos(int torneoId, ConfiguracionTorneo config, List<int> equipoIds);

        /// <summary>Elimina todos los partidos existentes del torneo y guarda los nuevos.</summary>
        Task<int> GuardarFixture(int torneoId, List<Partido> partidos);

        /// <summary>Retorna true si el torneo ya tiene partidos generados.</summary>
        Task<bool> TieneFixture(int torneoId);

        /// <summary>Cuenta partidos por estado para el torneo dado.</summary>
        Task<(int total, int programados, int jugados)> EstadisticasFixture(int torneoId);
    }
}
