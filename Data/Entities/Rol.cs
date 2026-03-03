using System.Collections.Generic;

namespace ControlTorneosFutbol.Data.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreNormalizado { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        // Relaciones
        public ICollection<UsuarioRol> UsuariosRoles { get; set; } = new List<UsuarioRol>();
    }

    public static class RolesConstantes
    {
        public const string Administrador = "Administrador";
        public const string Espectador = "Espectador";
        public const string Jugador = "Jugador";
        public const string Entrenador = "Entrenador";
        public const string Arbitro = "Arbitro";
    }
}