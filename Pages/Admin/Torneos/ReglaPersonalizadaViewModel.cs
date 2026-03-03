using System.ComponentModel.DataAnnotations;

namespace ControlTorneosFutbol.Models.ViewModels
{
    public class ReglaPersonalizadaViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El nombre de la regla es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Nombre de la Regla")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de regla es obligatorio")]
        [Display(Name = "Tipo de Regla")]
        public string Tipo { get; set; } = string.Empty;

        [Display(Name = "Regla Activa")]
        public bool Activa { get; set; } = true;

        [Display(Name = "Validación Automática")]
        public bool EsAutomatica { get; set; } = true;

        public int Orden { get; set; } = 0;

        [Display(Name = "Configuración Específica (JSON)")]
        public string? CondicionJson { get; set; }
    }

    // Plantillas predefinidas de reglas
    public static class ReglasPlantilla
    {
        public static List<ReglaPersonalizadaViewModel> ObtenerPlantillas()
        {
            return new List<ReglaPersonalizadaViewModel>
            {
                new ReglaPersonalizadaViewModel
                {
                    Nombre = "Suspensión por acumulación de tarjetas amarillas",
                    Descripcion = "Jugador suspendido automáticamente al acumular 3 tarjetas amarillas",
                    Tipo = "Suspension",
                    EsAutomatica = true,
                    CondicionJson = "{\"tarjetas_amarillas\": 3, \"partidos_suspension\": 1}"
                },
                new ReglaPersonalizadaViewModel
                {
                    Nombre = "Suspensión por tarjeta roja",
                    Descripcion = "Jugador suspendido 2 partidos por tarjeta roja directa",
                    Tipo = "Suspension",
                    EsAutomatica = true,
                    CondicionJson = "{\"tarjetas_rojas\": 1, \"partidos_suspension\": 2}"
                },
                new ReglaPersonalizadaViewModel
                {
                    Nombre = "Mínimo de jugadores nacionales",
                    Descripcion = "Al menos 8 jugadores nacionales en la alineación titular",
                    Tipo = "Restriccion",
                    EsAutomatica = true,
                    CondicionJson = "{\"jugadores_nacionales_minimo\": 8}"
                },
                new ReglaPersonalizadaViewModel
                {
                    Nombre = "Restricción de edad",
                    Descripcion = "Jugadores deben estar en el rango de edad de la categoría",
                    Tipo = "Validacion",
                    EsAutomatica = true,
                    CondicionJson = "{\"verificar_edad_categoria\": true}"
                },
                new ReglaPersonalizadaViewModel
                {
                    Nombre = "Fair Play Financiero",
                    Descripcion = "Equipo debe estar al día con pagos de inscripción",
                    Tipo = "Validacion",
                    EsAutomatica = true,
                    CondicionJson = "{\"verificar_pagos\": true}"
                },
                new ReglaPersonalizadaViewModel
                {
                    Nombre = "Documentación de jugadores",
                    Descripcion = "Todos los jugadores deben tener identificación válida registrada",
                    Tipo = "Validacion",
                    EsAutomatica = true,
                    CondicionJson = "{\"verificar_documentacion\": true}"
                }
            };
        }
    }
}