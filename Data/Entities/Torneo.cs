using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Torneo
    {
        public int Id { get; set; }
        public int LigaId { get; set; }
        public int TemporadaId { get; set; }
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Formato { get; set; } = string.Empty; // TodosContraTodos, EliminacionDirecta
        public string Estado { get; set; } = "Borrador"; // Borrador, Activo, Finalizado
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? NumeroEquiposMaximo { get; set; }
        public int NumeroEquiposMinimo { get; set; } = 2;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Campos para landing page pública
        public string? LogoUrl { get; set; }
        public string? ImagenUrl { get; set; }
        public string? DescripcionCompleta { get; set; }
        public string? Ubicacion { get; set; }
        public string? DireccionSede { get; set; }
        public string? UrlGrupoWhatsApp { get; set; }
        public string? UrlReglamento { get; set; }
        public string? ImagenesGaleria { get; set; }   // JSON array de URLs
        public string? Patrocinadores { get; set; }    // JSON array de URLs
        public string? Frase1 { get; set; }
        public string? Frase2 { get; set; }
        public string? Frase3 { get; set; }

        // Navegación
        public Liga Liga { get; set; } = null!;
        public Temporada Temporada { get; set; } = null!;
        public Categoria Categoria { get; set; } = null!;
        public ConfiguracionTorneo? ConfiguracionTorneo { get; set; }
        public ICollection<EquipoTorneo> EquiposTorneos { get; set; } = new List<EquipoTorneo>();
    }

    public static class FormatosTorneo
    {
        public const string TodosContraTodos = "TodosContraTodos";
        public const string EliminacionDirecta = "EliminacionDirecta";
        public const string GruposYEliminacion = "GruposYEliminacion";
    }

    public static class EstadosTorneo
    {
        public const string Borrador = "Borrador";
        public const string Activo = "Activo";
        public const string Finalizado = "Finalizado";
    }
}