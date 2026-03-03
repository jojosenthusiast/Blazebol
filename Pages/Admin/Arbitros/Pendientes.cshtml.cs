using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Arbitros
{
    public class PendientesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PendientesModel> _logger;

        public PendientesModel(ApplicationDbContext context, ILogger<PendientesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<ArbitroPendienteViewModel> ArbitrosPendientes { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            ArbitrosPendientes = await _context.Arbitros
                .Include(a => a.Persona)
                    .ThenInclude(p => p.Usuario)
                .Where(a => a.EstadoAprobacion == "Pendiente")
                .Select(a => new ArbitroPendienteViewModel
                {
                    ArbitroId = a.Id,
                    NombreCompleto = $"{a.Persona.Nombre} {a.Persona.ApellidoPaterno}",
                    Email = a.Persona.Usuario.Email,
                    NumeroLicencia = a.NumeroLicencia,
                    CategoriasArbitra = a.CategoriasArbitra ?? "No especificadas",
                    FechaSolicitud = a.Persona.Usuario.FechaCreacion
                })
                .OrderBy(a => a.FechaSolicitud)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAprobarAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var arbitro = await _context.Arbitros.FindAsync(id);
            if (arbitro == null)
            {
                return NotFound();
            }

            arbitro.EstadoAprobacion = "Aprobado";
            arbitro.FechaAprobacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Árbitro {ArbitroId} aprobado por admin", id);

            return RedirectToPage("/Admin/Arbitros/Pendientes", new { message = "Árbitro aprobado exitosamente" });
        }

        public async Task<IActionResult> OnPostRechazarAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var arbitro = await _context.Arbitros.FindAsync(id);
            if (arbitro == null)
            {
                return NotFound();
            }

            arbitro.EstadoAprobacion = "Rechazado";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Árbitro {ArbitroId} rechazado por admin", id);

            return RedirectToPage("/Admin/Arbitros/Pendientes", new { message = "Árbitro rechazado" });
        }
    }

    public class ArbitroPendienteViewModel
    {
        public int ArbitroId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NumeroLicencia { get; set; } = string.Empty;
        public string CategoriasArbitra { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
    }
}