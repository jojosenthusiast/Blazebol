using System;
using System.ComponentModel.DataAnnotations;

namespace ControlTorneosFutbol.Models.ViewModels
{
    public class TorneoViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre del torneo es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        [Display(Name = "Nombre del Torneo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La liga es obligatoria")]
        [Display(Name = "Liga")]
        public int LigaId { get; set; }

        [Required(ErrorMessage = "La temporada es obligatoria")]
        [Display(Name = "Temporada")]
        public int TemporadaId { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Display(Name = "Categoría/División")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El formato del torneo es obligatorio")]
        [Display(Name = "Formato")]
        public string Formato { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado del torneo es obligatorio")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Borrador";

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddMonths(3);

        [Range(2, 100, ErrorMessage = "El número mínimo de equipos debe estar entre 2 y 100")]
        [Display(Name = "Número Mínimo de Equipos")]
        public int NumeroEquiposMinimo { get; set; } = 2;

        [Range(2, 100, ErrorMessage = "El número máximo de equipos debe estar entre 2 y 100")]
        [Display(Name = "Número Máximo de Equipos")]
        public int? NumeroEquiposMaximo { get; set; }

        // Propiedades de navegación para mostrar en vistas
        public string? NombreLiga { get; set; }
        public string? NombreTemporada { get; set; }
        public string? NombreCategoria { get; set; }
    }
}