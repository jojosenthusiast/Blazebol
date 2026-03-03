using System;
using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Jugador
    {
        public int Id { get; set; }
        public Guid PersonaId { get; set; }
        public string? PosicionPreferida { get; set; }
        public int? NumeroCamiseta { get; set; }
        public decimal? Peso { get; set; }
        public decimal? Altura { get; set; }

        // Navegación
        public Persona Persona { get; set; } = null!;
        public ICollection<EquipoJugadorTorneo> EquiposJugadoresTorneos { get; set; } = new List<EquipoJugadorTorneo>();
    }

    public static class PosicionesJugador
    {
        public const string Portero = "Portero";
        public const string Defensa = "Defensa";
        public const string Medio = "Medio";
        public const string Delantero = "Delantero";
    }
}