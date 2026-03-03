using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Partidos
{
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public List<ArbitroDropdownItem> Arbitros { get; set; } = new();

        [BindProperty]
        public EditarPartidoInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var partido = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partido == null) return NotFound();

            await CargarArbitrosAsync();
            NombreLocal = partido.EquipoLocal.Nombre;
            NombreVisitante = partido.EquipoVisitante.Nombre;
            NombreTorneo = partido.Torneo.Nombre;

            Input = new EditarPartidoInput
            {
                Id = partido.Id,
                Estado = partido.Estado,
                GolesLocal = partido.GolesLocal,
                GolesVisitante = partido.GolesVisitante,
                Fecha = partido.Fecha,
                Sede = partido.Sede,
                ArbitroId = partido.ArbitroId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Torneo)
                .FirstOrDefaultAsync(p => p.Id == Input.Id);

            if (partido == null) return NotFound();

            NombreLocal = partido.EquipoLocal.Nombre;
            NombreVisitante = partido.EquipoVisitante.Nombre;
            NombreTorneo = partido.Torneo.Nombre;
            await CargarArbitrosAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Input.Estado == EstadosPartido.Finalizado &&
                (!Input.GolesLocal.HasValue || !Input.GolesVisitante.HasValue))
            {
                ModelState.AddModelError(string.Empty,
                    "Para finalizar el partido debes registrar los goles de ambos equipos.");
                return Page();
            }

            partido.Estado = Input.Estado;
            partido.Fecha = Input.Fecha;
            partido.Sede = string.IsNullOrWhiteSpace(Input.Sede) ? null : Input.Sede.Trim();
            partido.ArbitroId = Input.ArbitroId;
            partido.FechaActualizacion = DateTime.UtcNow;

            if (Input.Estado == EstadosPartido.Finalizado)
            {
                partido.GolesLocal = Input.GolesLocal;
                partido.GolesVisitante = Input.GolesVisitante;
            }
            else
            {
                partido.GolesLocal = null;
                partido.GolesVisitante = null;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Partidos/Details", new { id = partido.Id });
        }

        private async Task CargarArbitrosAsync()
        {
            Arbitros = await _context.Arbitros
                .Include(a => a.Persona)
                .Where(a => a.EstadoAprobacion == EstadosAprobacionArbitro.Aprobado)
                .OrderBy(a => a.Persona.ApellidoPaterno)
                .Select(a => new ArbitroDropdownItem
                {
                    Id = a.Id,
                    NombreCompleto = $"{a.Persona.Nombre} {a.Persona.ApellidoPaterno}"
                })
                .ToListAsync();
        }
    }

    public class EditarPartidoInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = EstadosPartido.Programado;

        [Display(Name = "Goles Local")]
        [Range(0, 99, ErrorMessage = "Valor entre 0 y 99")]
        public int? GolesLocal { get; set; }

        [Display(Name = "Goles Visitante")]
        [Range(0, 99, ErrorMessage = "Valor entre 0 y 99")]
        public int? GolesVisitante { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha y Hora")]
        public DateTime Fecha { get; set; }

        [MaxLength(200)]
        [Display(Name = "Sede")]
        public string? Sede { get; set; }

        [Display(Name = "Árbitro")]
        public int? ArbitroId { get; set; }
    }
}
