using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Usuarios
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ApplicationDbContext context, ILogger<DetailsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public UsuarioDetalleViewModel? Usuario { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Persona)
                    .ThenInclude(p => p.Jugador)
                .Include(u => u.Persona)
                    .ThenInclude(p => p.Entrenador)
                        .ThenInclude(e => e.EquiposEntrenadores)
                            .ThenInclude(ee => ee.Equipo)
                .Include(u => u.Persona)
                    .ThenInclude(p => p.Arbitro)
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return Page();
            }

            var rol = usuario.UsuariosRoles.FirstOrDefault()?.Rol.Nombre ?? "Sin rol";

            Usuario = new UsuarioDetalleViewModel
            {
                UsuarioId = usuario.Id,
                Email = usuario.Email,
                NombreCompleto = usuario.Persona != null 
                    ? $"{usuario.Persona.Nombre} {usuario.Persona.ApellidoPaterno} {usuario.Persona.ApellidoMaterno ?? ""}".Trim()
                    : usuario.Email,
                Telefono = usuario.Persona?.Telefono,
                Rol = rol,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion,
                ContadorAccesosFallidos = usuario.ContadorAccesosFallidos,
                FechaBloqueoFin = usuario.FechaBloqueoFin
            };

            // Información específica por rol
            if (rol == "Jugador" && usuario.Persona?.Jugador != null)
            {
                Usuario.JugadorInfo = new JugadorInfoViewModel
                {
                    PosicionPreferida = usuario.Persona.Jugador.PosicionPreferida,
                    NumeroCamiseta = usuario.Persona.Jugador.NumeroCamiseta
                };
            }
            else if (rol == "Entrenador" && usuario.Persona?.Entrenador != null)
            {
                var equipoActual = usuario.Persona.Entrenador.EquiposEntrenadores
                    .Where(ee => ee.Activo)
                    .OrderByDescending(ee => ee.FechaInicio)
                    .FirstOrDefault();

                Usuario.EntrenadorInfo = new EntrenadorInfoViewModel
                {
                    NumeroLicencia = usuario.Persona.Entrenador.NumeroLicencia,
                    EquipoNombre = equipoActual?.Equipo?.Nombre
                };
            }
            else if (rol == "Arbitro" && usuario.Persona?.Arbitro != null)
            {
                Usuario.ArbitroInfo = new ArbitroInfoViewModel
                {
                    NumeroLicencia = usuario.Persona.Arbitro.NumeroLicencia,
                    CategoriasArbitra = usuario.Persona.Arbitro.CategoriasArbitra ?? "No especificadas",
                    EstadoAprobacion = usuario.Persona.Arbitro.EstadoAprobacion,
                    FechaAprobacion = usuario.Persona.Arbitro.FechaAprobacion
                };
            }

            return Page();
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

            usuario.FechaBloqueoFin = null;
            usuario.ContadorAccesosFallidos = 0;
            usuario.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario {Email} ({UsuarioId}) desbloqueado por admin desde página de detalles", usuario.Email, id);

            return RedirectToPage("/Admin/Usuarios/Details", new { id = id, message = "Usuario desbloqueado exitosamente" });
        }
    }

    public class UsuarioDetalleViewModel
    {
        public Guid UsuarioId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int ContadorAccesosFallidos { get; set; }
        public DateTime? FechaBloqueoFin { get; set; }
        
        public JugadorInfoViewModel? JugadorInfo { get; set; }
        public EntrenadorInfoViewModel? EntrenadorInfo { get; set; }
        public ArbitroInfoViewModel? ArbitroInfo { get; set; }
    }

    public class JugadorInfoViewModel
    {
        public string? PosicionPreferida { get; set; }
        public int? NumeroCamiseta { get; set; }
    }

    public class EntrenadorInfoViewModel
    {
        public string? NumeroLicencia { get; set; }
        public string? EquipoNombre { get; set; }
    }

    public class ArbitroInfoViewModel
    {
        public string NumeroLicencia { get; set; } = string.Empty;
        public string CategoriasArbitra { get; set; } = string.Empty;
        public string EstadoAprobacion { get; set; } = string.Empty;
        public DateTime? FechaAprobacion { get; set; }
    }
}