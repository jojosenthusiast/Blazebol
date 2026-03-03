using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? EdadMinima { get; set; }
        public int? EdadMaxima { get; set; }
        public string? Descripcion { get; set; }
        public int Orden { get; set; } = 0;

        // Navegación
        public ICollection<Torneo> Torneos { get; set; } = new List<Torneo>();
    }
}