namespace ControlTorneosFutbol.Models.DTOs
{
    public class ResultadoOperacionDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public object? Datos { get; set; }

        public static ResultadoOperacionDto Exito(string mensaje, object? datos = null)
        {
            return new ResultadoOperacionDto
            {
                Exitoso = true,
                Mensaje = mensaje,
                Datos = datos
            };
        }

        public static ResultadoOperacionDto Error(string mensaje)
        {
            return new ResultadoOperacionDto
            {
                Exitoso = false,
                Mensaje = mensaje
            };
        }
    }
}