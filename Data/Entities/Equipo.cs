using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Equipo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? ColorPrimario { get; set; }
        public string? ColorSecundario { get; set; }
        public string? Sede { get; set; }
        public DateTime? FechaFundacion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;
        /// <summary>Libre | Masculino | Femenino | Juvenil | Amateur | Veterano</summary>
        public string TipoEquipo { get; set; } = TiposEquipo.Libre;

        // Navegación
        public ICollection<EquipoTorneo> EquiposTorneos { get; set; } = new List<EquipoTorneo>();
        public ICollection<EquipoEntrenador> EquiposEntrenadores { get; set; } = new List<EquipoEntrenador>();
    }

    public static class TiposEquipo
    {
        public const string Libre     = "Libre";
        public const string Masculino = "Masculino";
        public const string Femenino  = "Femenino";
        public const string Juvenil   = "Juvenil";  // Sub-18
        public const string Amateur   = "Amateur";  // 18+
        public const string Veterano  = "Veterano"; // 40+

        public static readonly Dictionary<string, string> Etiquetas = new()
        {
            [Libre]     = "Libre (sin restricciones)",
            [Masculino] = "Masculino",
            [Femenino]  = "Femenino",
            [Juvenil]   = "Juvenil (Sub-18)",
            [Amateur]   = "Amateur (18+)",
            [Veterano]  = "Veterano (40+)",
        };

        /// <summary>Retorna (ok, razon) validando compatibilidad jugador ↔ tipo de equipo.</summary>
        public static (bool ok, string razon) ValidarJugador(string tipoEquipo, string? genero, DateTime? fechaNacimiento)
        {
            int edad = fechaNacimiento.HasValue
                ? (int)((DateTime.UtcNow - fechaNacimiento.Value).TotalDays / 365.25)
                : -1;

            return tipoEquipo switch
            {
                Femenino when genero != GenerosPersona.Femenino
                    => (false, "El equipo es femenino — solo jugadoras con género Femenino pueden inscribirse."),
                Masculino when genero == GenerosPersona.Femenino
                    => (false, "El equipo es masculino — no acepta jugadoras con género Femenino."),
                Juvenil when edad >= 0 && edad >= 18
                    => (false, $"El equipo es Juvenil (Sub-18) — el jugador tiene {edad} años y supera el límite de edad."),
                Veterano when edad >= 0 && edad < 40
                    => (false, $"El equipo es Veterano (40+) — el jugador tiene {edad} años y no cumple el mínimo de edad (40)."),
                _ => (true, string.Empty)
            };
        }
    }
}
