using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Entrenador.Plantilla
{
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditarModel> _logger;

        public EditarModel(ApplicationDbContext context, ILogger<EditarModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public EditarInput Input { get; set; } = new();

        public string NombreJugador { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public string NombreEquipo { get; set; } = string.Empty;
        public string? PosicionJugador { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var ejt = await CargarEjtAsync(id);
            if (ejt == null) return NotFound();

            Input = new EditarInput
            {
                Id = ejt.Id,
                NumeroCamiseta = ejt.NumeroCamiseta,
                TorneoId = ejt.TorneoId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                var ejtReload = await CargarEjtAsync(Input.Id);
                if (ejtReload == null) return NotFound();
                return Page();
            }

            var ejt = await CargarEjtAsync(Input.Id);
            if (ejt == null) return NotFound();

            // Validar que la camiseta no esté tomada por otro jugador en el mismo equipo-torneo
            var camisetaOcupada = await _context.EquiposJugadoresTorneos
                .AnyAsync(e => e.EquipoId == ejt.EquipoId
                            && e.TorneoId == ejt.TorneoId
                            && e.NumeroCamiseta == Input.NumeroCamiseta
                            && e.Activo
                            && e.Id != ejt.Id);

            if (camisetaOcupada)
            {
                ErrorMessage = $"El número de camiseta {Input.NumeroCamiseta} ya está asignado a otro jugador en este torneo.";
                await CargarEjtAsync(Input.Id);
                return Page();
            }

            ejt.NumeroCamiseta = Input.NumeroCamiseta;
            await _context.SaveChangesAsync();

            _logger.LogInformation("EJT {Id} actualizado: camiseta → {Camiseta}", ejt.Id, Input.NumeroCamiseta);

            return RedirectToPage("/Entrenador/Plantilla/Index",
                new { torneoId = ejt.TorneoId, message = "Datos del jugador actualizados." });
        }

        private async Task<Data.Entities.EquipoJugadorTorneo?> CargarEjtAsync(int id)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return null;

            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Entrenador == null) return null;

            var equipoEntrenador = await _context.EquiposEntrenadores
                .Where(ee => ee.EntrenadorId == persona.Entrenador.Id && ee.Activo)
                .FirstOrDefaultAsync();

            if (equipoEntrenador == null) return null;

            var ejt = await _context.EquiposJugadoresTorneos
                .Include(e => e.Jugador)
                    .ThenInclude(j => j.Persona)
                .Include(e => e.Torneo)
                .Include(e => e.Equipo)
                .FirstOrDefaultAsync(e => e.Id == id && e.EquipoId == equipoEntrenador.EquipoId);

            if (ejt != null)
            {
                NombreJugador = ejt.Jugador.Persona.Nombre + " " + ejt.Jugador.Persona.ApellidoPaterno;
                NombreTorneo = ejt.Torneo.Nombre;
                NombreEquipo = ejt.Equipo.Nombre;
                PosicionJugador = ejt.Jugador.PosicionPreferida;
                FechaInscripcion = ejt.FechaInscripcion;
            }

            return ejt;
        }
    }

    public class EditarInput
    {
        public int Id { get; set; }
        public int TorneoId { get; set; }

        [Required(ErrorMessage = "El número de camiseta es obligatorio")]
        [Range(1, 99, ErrorMessage = "El número debe estar entre 1 y 99")]
        [Display(Name = "Número de Camiseta")]
        public int NumeroCamiseta { get; set; }
    }
}
