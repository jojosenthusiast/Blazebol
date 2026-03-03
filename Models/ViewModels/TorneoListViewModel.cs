using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Models.ViewModels
{
    public class TorneoListViewModel
    {
        public List<TorneoItemViewModel> Torneos { get; set; } = new List<TorneoItemViewModel>();
        
        // Filtros
        public string? FiltroEstado { get; set; }
        public int? FiltroAnio { get; set; }
        public int? FiltroCategoriaId { get; set; }

        // Datos para los dropdowns de filtros
        public List<int> AniosDisponibles { get; set; } = new List<int>();
        public Dictionary<int, string> CategoriasDisponibles { get; set; } = new Dictionary<int, string>();
    }

    public class TorneoItemViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreLiga { get; set; } = string.Empty;
        public string NombreTemporada { get; set; } = string.Empty;
        public string NombreCategoria { get; set; } = string.Empty;
        public string Formato { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int NumeroEquiposMinimo { get; set; } = 2;
        public int NumeroEquiposInscritos { get; set; }
        public int? NumeroEquiposMaximo { get; set; }
        public bool PuedeEditar { get; set; }
    }
}