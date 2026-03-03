using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, IPdfService pdfService, ILogger<IndexModel> logger)
        {
            _context = context;
            _pdfService = pdfService;
            _logger = logger;
        }

        public List<UsuarioListItemViewModel> Usuarios { get; set; } = new();
        public string? FiltroRol { get; set; }
        public bool? FiltroActivo { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? rol, bool? activo, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            FiltroRol = rol;
            FiltroActivo = activo;

            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            var query = _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(rol))
            {
                query = query.Where(u => u.UsuariosRoles.Any(ur => ur.Rol.Nombre == rol));
            }

            if (activo.HasValue)
            {
                query = query.Where(u => u.Activo == activo.Value);
            }

            Usuarios = await query
                .Select(u => new UsuarioListItemViewModel
                {
                    UsuarioId = u.Id,
                    Email = u.Email,
                    NombreCompleto = u.Persona != null 
                        ? $"{u.Persona.Nombre} {u.Persona.ApellidoPaterno}" 
                        : u.Email,
                    Rol = u.UsuariosRoles.FirstOrDefault() != null 
                        ? u.UsuariosRoles.FirstOrDefault()!.Rol.Nombre 
                        : "Sin rol",
                    Activo = u.Activo,
                    FechaCreacion = u.FechaCreacion,
                    ContadorAccesosFallidos = u.ContadorAccesosFallidos,
                    FechaBloqueoFin = u.FechaBloqueoFin
                })
                .OrderByDescending(u => u.FechaCreacion)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDesactivarAsync(Guid id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = false;
            usuario.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario {UsuarioId} desactivado por admin", id);

            return RedirectToPage("/Admin/Usuarios/Index", new { message = "Usuario desactivado exitosamente" });
        }

        public async Task<IActionResult> OnPostActivarAsync(Guid id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = true;
            usuario.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario {UsuarioId} activado por admin", id);

            return RedirectToPage("/Admin/Usuarios/Index", new { message = "Usuario activado exitosamente" });
        }

        public async Task<IActionResult> OnPostDesbloquearAsync(Guid id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Desbloquear y resetear contador
            usuario.FechaBloqueoFin = null;
            usuario.ContadorAccesosFallidos = 0;
            usuario.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario {Email} ({UsuarioId}) desbloqueado manualmente por admin", usuario.Email, id);

            return RedirectToPage("/Admin/Usuarios/Index", new { message = "Usuario desbloqueado exitosamente" });
        }

        public async Task<IActionResult> OnPostExportarPDFAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var usuarios = await _context.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .OrderBy(u => u.Email)
                .ToListAsync();

            var pdfBytes = _pdfService.GenerateUsuariosReport(usuarios);

            return File(pdfBytes, "application/pdf", $"Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }

    public class UsuarioListItemViewModel
    {
        public Guid UsuarioId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        // Propiedades de seguridad
        public int ContadorAccesosFallidos { get; set; }
        public DateTime? FechaBloqueoFin { get; set; }
        public bool EstaBloqueado => FechaBloqueoFin.HasValue && FechaBloqueoFin.Value > DateTime.UtcNow;
    }
}