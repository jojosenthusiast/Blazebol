using System.Collections.Generic;

namespace ControlTorneosFutbol.Models.ViewModels
{
    public class DashboardAdminViewModel
    {
        public int TotalTorneos { get; set; }
        public int TorneosActivos { get; set; }
        public int TotalEquipos { get; set; }
        public int TotalJugadores { get; set; }
        public int TotalArbitros { get; set; }
        public int ArbitrosPendientesAprobacion { get; set; }
        public List<TorneoItemViewModel> UltimosTorneos { get; set; } = new List<TorneoItemViewModel>();
        public int AccesosHoy { get; set; }
        public int IntentosFallidos { get; set; }
        public int UsuariosBloqueados { get; set; }
    }

    public class DashboardJugadorViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string? EquipoActual { get; set; }
        public string? TorneoActual { get; set; }
        public int PartidosJugados { get; set; }
        public int Goles { get; set; }
        public int Asistencias { get; set; }
        public int TarjetasAmarillas { get; set; }
        public int TarjetasRojas { get; set; }
        public List<string> ProximosPartidos { get; set; } = new List<string>();
    }

    public class DashboardEntrenadorViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string? NombreEquipo { get; set; }
        public int JugadoresEnPlantilla { get; set; }
        public int PartidosJugados { get; set; }
        public int Victorias { get; set; }
        public int Empates { get; set; }
        public int Derrotas { get; set; }
        public List<string> ProximosPartidos { get; set; } = new List<string>();
    }

    public class DashboardArbitroViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoAprobacion { get; set; } = string.Empty;
        public int PartidosArbitrados { get; set; }
        public List<string> ProximosPartidos { get; set; } = new List<string>();
    }

    public class DashboardEspectadorViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public int TorneosActivos { get; set; }
        public List<TorneoItemViewModel> TorneosRecientes { get; set; } = new List<TorneoItemViewModel>();
    }
}