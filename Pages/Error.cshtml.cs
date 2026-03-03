using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace ControlTorneosFutbol.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string Message { get; set; } = "Ha ocurrido un error inesperado.";
        public new int StatusCode { get; set; } = 500;  // <-- Agregado 'new' para resolver warning

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? statusCode = null)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            
            if (statusCode.HasValue)
            {
                StatusCode = statusCode.Value;
            }
            
            _logger.LogWarning("Error page accessed. Status Code: {StatusCode}, Request ID: {RequestId}", 
                StatusCode, RequestId);
        }
    }
}