using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Espectador
{
    public class PreferenciasModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PreferenciasModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int? EquipoFavoritoId { get; set; }

        [BindProperty]
        public bool NotificacionesEmail { get; set; } = true;

        public Dictionary<int, string> EquiposDisponibles { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Espectador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Cargar preferencias existentes
            var preferencia = await _context.PreferenciasEspectador
                .FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (preferencia != null)
            {
                EquipoFavoritoId = preferencia.EquipoFavoritoId;
                NotificacionesEmail = preferencia.NotificacionesEmail;
            }

            // Cargar equipos disponibles
            EquiposDisponibles = await _context.Equipos
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .ToDictionaryAsync(e => e.Id, e => e.Nombre);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Espectador"))
            {
                return RedirectToPage("/Account/Login");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Buscar preferencia existente
            var preferencia = await _context.PreferenciasEspectador
                .FirstOrDefaultAsync(p => p.UsuarioId == userId);

            if (preferencia == null)
            {
                // Crear nueva preferencia
                preferencia = new PreferenciaEspectador
                {
                    UsuarioId = userId,
                    EquipoFavoritoId = EquipoFavoritoId,
                    NotificacionesEmail = NotificacionesEmail,
                    FechaCreacion = DateTime.UtcNow
                };
                _context.PreferenciasEspectador.Add(preferencia);
            }
            else
            {
                // Actualizar existente
                preferencia.EquipoFavoritoId = EquipoFavoritoId;
                preferencia.NotificacionesEmail = NotificacionesEmail;
                preferencia.FechaActualizacion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Recargar equipos para mostrar el formulario de nuevo
            EquiposDisponibles = await _context.Equipos
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .ToDictionaryAsync(e => e.Id, e => e.Nombre);

            SuccessMessage = "Preferencias guardadas exitosamente";
            return Page();
        }
    }
}