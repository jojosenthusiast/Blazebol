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

            // Update Persona fields
            persona.Nombre = Input.Nombre.Trim();
            persona.ApellidoPaterno = Input.ApellidoPaterno.Trim();
            persona.ApellidoMaterno = string.IsNullOrWhiteSpace(Input.ApellidoMaterno)
                ? null : Input.ApellidoMaterno.Trim();
            persona.Telefono = string.IsNullOrWhiteSpace(Input.Telefono)
                ? null : Input.Telefono.Trim();
            persona.FechaActualizacion = DateTime.UtcNow;

            // Update Jugador fields
            persona.Jugador.PosicionPreferida = string.IsNullOrWhiteSpace(Input.PosicionPreferida)
                ? null : Input.PosicionPreferida;
            persona.Jugador.NumeroCamiseta = Input.NumeroCamiseta;

            await _context.SaveChangesAsync();

            // Refresh session name with updated data
            HttpContext.Session.SetUserSession(
                userId.Value,
                "Jugador",
                $"{persona.Nombre} {persona.ApellidoPaterno}",
                HttpContext.Session.GetUserEmail() ?? string.Empty);

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";
            SuccessMessage = "Perfil actualizado correctamente.";
            return Page();
        }

        private void PopulateInput(Persona persona)
        {
            Input = new EditarPerfilInput
            {
                Nombre = persona.Nombre,
                ApellidoPaterno = persona.ApellidoPaterno,
                ApellidoMaterno = persona.ApellidoMaterno,
                Telefono = persona.Telefono,
                PosicionPreferida = persona.Jugador?.PosicionPreferida,
                NumeroCamiseta = persona.Jugador?.NumeroCamiseta
            };
        }

        private async Task RecargarNombreAsync(Guid userId)
        {
            var persona = await _context.Personas
                .FirstOrDefaultAsync(p => p.UsuarioId == userId);
            NombreCompleto = persona != null
                ? $"{persona.Nombre} {persona.ApellidoPaterno}"
                : string.Empty;
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
