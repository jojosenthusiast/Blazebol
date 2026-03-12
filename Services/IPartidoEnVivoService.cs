using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public interface IPartidoEnVivoService
    {
        Task<bool> IniciarPartidoAsync(int partidoId, int arbitroId, INotificacionService notif);
        Task<bool> RegistrarIncidenciaAsync(int partidoId, string tipo, int equipoId, int? ejtJugadorId, int? minuto, string? desc, bool esAutogol, bool esPenal, int? ejtJugadorSalidaId = null);
        Task RecalcularGolesAsync(int partidoId);
        Task<bool> FinalizarPartidoAsync(int partidoId, int arbitroId, INotificacionService notif);
        Task<List<IncidenciaPartido>> GetIncidenciasAsync(int partidoId);
        Task<PartidoEnVivoVM> GetEstadoEnVivoAsync(int partidoId);
    }

    public class PartidoEnVivoVM
    {
        public int PartidoId { get; set; }
        public int EquipoLocalId { get; set; }
        public int EquipoVisitanteId { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string? LogoLocal { get; set; }
        public string? LogoVisitante { get; set; }
        public string? AbrevLocal { get; set; }
        public string? AbrevVisitante { get; set; }
        public string? ColorLocal { get; set; }
        public string? ColorVisitante { get; set; }
        public int GolesLocal { get; set; }
        public int GolesVisitante { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? MinutoActual { get; set; }
        public DateTime? FechaInicioReal { get; set; }
        public List<IncidenciaVM> Incidencias { get; set; } = new();
        public List<ConvocadoVM> JugadoresLocal { get; set; } = new();
        public List<ConvocadoVM> JugadoresVisitante { get; set; } = new();
    }

    public class ConvocadoVM
    {
        public int EjtId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int NumeroCamiseta { get; set; }
        public string? PosicionAsignada { get; set; }
        public string Rol { get; set; } = string.Empty;
        public int TarjetasAmarillas { get; set; }
        public bool TarjetaRoja { get; set; }
        public bool EstaEnCampo { get; set; }
    }

    public class IncidenciaVM
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? Minuto { get; set; }
        public string? NombreJugador { get; set; }
        public string? NombreJugadorSalida { get; set; }
        public bool EsAutogol { get; set; }
        public bool EsPenal { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
