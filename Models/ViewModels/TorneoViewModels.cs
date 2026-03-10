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

        // Campos para landing page pública
        [Display(Name = "URL del Logo")]
        public string? LogoUrl { get; set; }

        [Display(Name = "URL de Imagen Principal")]
        public string? ImagenUrl { get; set; }

        [Display(Name = "Descripción Completa")]
        public string? DescripcionCompleta { get; set; }

        [Display(Name = "Ubicación")]
        [StringLength(200)]
        public string? Ubicacion { get; set; }

        [Display(Name = "Dirección de la Sede")]
        public string? DireccionSede { get; set; }

        [Display(Name = "Link Grupo WhatsApp")]
        public string? UrlGrupoWhatsApp { get; set; }

        [Display(Name = "URL Reglamento (PDF)")]
        public string? UrlReglamento { get; set; }

        [Display(Name = "Galería (JSON de URLs)")]
        public string? ImagenesGaleria { get; set; }

        [Display(Name = "Patrocinadores (JSON de URLs)")]
        public string? Patrocinadores { get; set; }

        [Display(Name = "Frase 1")]
        [StringLength(100)]
        public string? Frase1 { get; set; }

        [Display(Name = "Frase 2")]
        [StringLength(100)]
        public string? Frase2 { get; set; }

        [Display(Name = "Frase 3")]
        [StringLength(100)]
        public string? Frase3 { get; set; }

        // Configuración de fixture automático
        [Display(Name = "Ida y Vuelta")]
        public bool IdaYVuelta { get; set; } = false;

        [Display(Name = "Campo Neutral")]
        public bool CampoNeutral { get; set; } = false;

        [Display(Name = "Equilibrar Local/Visitante")]
        public bool EquilibrioLocalVisitante { get; set; } = true;

        [Display(Name = "Número de Grupos")]
        [Range(2, 16)]
        public int? NumeroGrupos { get; set; }

        [Display(Name = "Equipos por Grupo")]
        [Range(2, 20)]
        public int? EquiposPorGrupo { get; set; }

        [Display(Name = "Clasifican por Grupo")]
        [Range(1, 10)]
        public int? ClasificanPorGrupo { get; set; } = 2;
    }

    // ViewModel para tarjeta pública de torneo
    public class TorneoPublicoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string LigaNombre { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public string? LogoUrl { get; set; }
        public int EquiposInscritos { get; set; }
        public int? EquiposMaximo { get; set; }

        public string EstadoDisplay => Estado switch
        {
            "Activo"     => "En curso",
            "Borrador"   => "Próximamente",
            "Finalizado" => "Finalizado",
            _            => Estado
        };

        public string BadgeClass => Estado switch
        {
            "Activo"     => "torneo-badge--activo",
            "Borrador"   => "torneo-badge--proximo",
            "Finalizado" => "torneo-badge--finalizado",
            _            => "torneo-badge--proximo"
        };
    }

    // ViewModel detalle público de torneo
    public class TorneoDetallePublicoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string LigaNombre { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public string Formato { get; set; } = string.Empty;
        public int EquiposInscritos { get; set; }
        public int? EquiposMaximo { get; set; }
        public string? LogoUrl { get; set; }
        public string? ImagenUrl { get; set; }
        public string? DescripcionCompleta { get; set; }
        public string? Ubicacion { get; set; }
        public string? DireccionSede { get; set; }
        public string? UrlGrupoWhatsApp { get; set; }
        public string? UrlReglamento { get; set; }
        public List<string> ImagenesGaleria { get; set; } = new();
        public List<string> Patrocinadores { get; set; } = new();
        public string? Frase1 { get; set; }
        public string? Frase2 { get; set; }
        public string? Frase3 { get; set; }
        public List<EquipoInscritoViewModel> Equipos { get; set; } = new();
        public List<PartidoPublicoViewModel> Partidos { get; set; } = new();
        public List<ClasificacionPublicaViewModel> Clasificacion { get; set; } = new();

        public string FormatoDisplay => Formato switch
        {
            "TodosContraTodos"   => "Todos contra Todos",
            "EliminacionDirecta" => "Eliminación Directa",
            "GruposYEliminacion" => "Grupos + Eliminación",
            _                    => Formato
        };
    }

    public class EquipoInscritoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? EntrenadorNombre { get; set; }
    }

    public class ClasificacionPublicaViewModel
    {
        public int Posicion { get; set; }
        public int EquipoId { get; set; }
        public string EquipoNombre { get; set; } = string.Empty;
        public string? EquipoLogo { get; set; }
        public string? EquipoAbreviacion { get; set; }
        public string? Grupo { get; set; }
        public int PJ { get; set; }
        public int G { get; set; }
        public int E { get; set; }
        public int P { get; set; }
        public int GF { get; set; }
        public int GC { get; set; }
        public int GD => GF - GC;
        public int Pts { get; set; }
    }

    public class PartidoPublicoViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string? Sede { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string EquipoLocalNombre { get; set; } = string.Empty;
        public string? EquipoLocalLogo { get; set; }
        public string? EquipoLocalAbreviacion { get; set; }
        public string EquipoVisitanteNombre { get; set; } = string.Empty;
        public string? EquipoVisitanteLogo { get; set; }
        public string? EquipoVisitanteAbreviacion { get; set; }
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }

        public bool EsFinalizado => Estado == "Finalizado";
        public bool EsEnCurso   => Estado == "EnCurso";
    }
}