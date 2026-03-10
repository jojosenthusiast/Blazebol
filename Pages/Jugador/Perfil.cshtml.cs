using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using System.ComponentModel.DataAnnotations;

namespace ControlTorneosFutbol.Pages.Jugador
{
    public class PerfilModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PerfilModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreCompleto { get; set; } = string.Empty;
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public EditarPerfilInput Input { get; set; } = new();

        // ── GET normal (página completa) ─────────────────────────────────
        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Jugador == null)
                return RedirectToPage("/Error");

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";
            PopulateInput(persona);
            return Page();
        }

        // ── GET handler AJAX: devuelve datos del perfil como JSON ─────────
        public async Task<IActionResult> OnGetDatosAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return new JsonResult(new { error = "No autorizado" }) { StatusCode = 401 };

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return new JsonResult(new { error = "Sin sesión" }) { StatusCode = 401 };

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null)
                return new JsonResult(new { error = "Perfil no encontrado" }) { StatusCode = 404 };

            return new JsonResult(new
            {
                nombre           = persona.Nombre,
                apellidoPaterno  = persona.ApellidoPaterno,
                apellidoMaterno  = persona.ApellidoMaterno ?? "",
                telefono         = persona.Telefono ?? "",
                posicionPreferida = persona.Jugador?.PosicionPreferida ?? "",
                numeroCamiseta   = persona.Jugador?.NumeroCamiseta?.ToString() ?? ""
            });
        }

        // ── POST normal (página completa) ────────────────────────────────
        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                await RecargarNombreAsync(userId.Value);
                return Page();
            }

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Jugador == null)
                return RedirectToPage("/Error");

            await ActualizarPersonaAsync(persona);

            HttpContext.Session.SetUserSession(
                userId.Value, "Jugador",
                $"{persona.Nombre} {persona.ApellidoPaterno}",
                HttpContext.Session.GetUserEmail() ?? string.Empty);

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";
            SuccessMessage = "Perfil actualizado correctamente.";
            return Page();
        }

        // ── POST handler AJAX: guarda el perfil y responde JSON ───────────
        public async Task<IActionResult> OnPostGuardarAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return new JsonResult(new { ok = false, error = "No autorizado" }) { StatusCode = 401 };

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return new JsonResult(new { ok = false, error = "Sin sesión" }) { StatusCode = 401 };

            // Re-bind Input desde el form
            await TryUpdateModelAsync(Input, "Input");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return new JsonResult(new { ok = false, errors });
            }

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Jugador == null)
                return new JsonResult(new { ok = false, error = "Perfil no encontrado" }) { StatusCode = 404 };

            await ActualizarPersonaAsync(persona);

            HttpContext.Session.SetUserSession(
                userId.Value, "Jugador",
                $"{persona.Nombre} {persona.ApellidoPaterno}",
                HttpContext.Session.GetUserEmail() ?? string.Empty);

            return new JsonResult(new
            {
                ok           = true,
                nombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}"
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────
        private async Task ActualizarPersonaAsync(Persona persona)
        {
            persona.Nombre           = Input.Nombre.Trim();
            persona.ApellidoPaterno  = Input.ApellidoPaterno.Trim();
            persona.ApellidoMaterno  = string.IsNullOrWhiteSpace(Input.ApellidoMaterno) ? null : Input.ApellidoMaterno.Trim();
            persona.Telefono         = string.IsNullOrWhiteSpace(Input.Telefono)         ? null : Input.Telefono.Trim();
            persona.FechaActualizacion = DateTime.UtcNow;

            persona.Jugador!.PosicionPreferida = string.IsNullOrWhiteSpace(Input.PosicionPreferida) ? null : Input.PosicionPreferida;
            persona.Jugador!.NumeroCamiseta    = Input.NumeroCamiseta;

            await _context.SaveChangesAsync();
        }

        private void PopulateInput(Persona persona)
        {
            Input = new EditarPerfilInput
            {
                Nombre            = persona.Nombre,
                ApellidoPaterno   = persona.ApellidoPaterno,
                ApellidoMaterno   = persona.ApellidoMaterno,
                Telefono          = persona.Telefono,
                PosicionPreferida = persona.Jugador?.PosicionPreferida,
                NumeroCamiseta    = persona.Jugador?.NumeroCamiseta
            };
        }

        private async Task RecargarNombreAsync(Guid userId)
        {
            var persona = await _context.Personas.FirstOrDefaultAsync(p => p.UsuarioId == userId);
            NombreCompleto = persona != null ? $"{persona.Nombre} {persona.ApellidoPaterno}" : string.Empty;
        }
    }

    public class EditarPerfilInput
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Apellido Materno")]
        public string? ApellidoMaterno { get; set; }

        [MaxLength(20, ErrorMessage = "Máximo 20 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Posición Preferida")]
        public string? PosicionPreferida { get; set; }

        [Range(1, 99, ErrorMessage = "El número debe estar entre 1 y 99")]
        [Display(Name = "Número de Camiseta")]
        public int? NumeroCamiseta { get; set; }
    }
}
