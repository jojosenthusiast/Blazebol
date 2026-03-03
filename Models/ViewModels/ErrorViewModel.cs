namespace ControlTorneosFutbol.Models.ViewModels
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string Message { get; set; } = "Ha ocurrido un error inesperado.";
        public int StatusCode { get; set; } = 500;
    }
}