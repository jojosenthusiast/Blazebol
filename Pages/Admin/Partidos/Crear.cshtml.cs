using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Partidos
{
    public class CrearModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CrearModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TorneoDropdownItem> Torneos { get; set; } = new();
        public List<ArbitroDropdownItem> Arbitros { get; set; } = new();
        // JSON serialized: { torneoId: [{id, nombre}] } — used by JS to filter equipo dropdowns
        public string EquiposPorTorneoJson { get; set; } = "{}";
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public CrearPartidoInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            await CargarDatosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            if (Input.EquipoLocalId == Input.EquipoVisitanteId)
            {
                ModelState.AddModelError(string.Empty, "El equipo local y el visitante deben ser diferentes.");
                await CargarDatosAsync();
                return Page();
            }

            // Verify both teams are enrolled in the selected torneo
            var equiposEnTorneo = await _context.EquiposTorneos
                .Where(et => et.TorneoId == Input.TorneoId)
                .Select(et => et.EquipoId)
                .ToListAsync();

            if (!equiposEnTorneo.Contains(Input.EquipoLocalId))
            {
                ModelState.AddModelError(string.Empty, "El equipo local no está inscrito en el torneo seleccionado.");
                await CargarDatosAsync();
                return Page();
            }

            if (!equiposEnTorneo.Contains(Input.EquipoVisitanteId))
            {
                ModelState.AddModelError(string.Empty, "El equipo visitante no está inscrito en el torneo seleccionado.");
                await CargarDatosAsync();
                return Page();
            }

            var partido = new Partido
            {
                TorneoId = Input.TorneoId,
                EquipoLocalId = Input.EquipoLocalId,
                EquipoVisitanteId = Input.EquipoVisitanteId,
                ArbitroId = Input.ArbitroId,
                Fecha = Input.Fecha,
                Sede = string.IsNullOrWhiteSpace(Input.Sede) ? null : Input.Sede.Trim(),
                Estado = EstadosPartido.Programado,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Partidos.Add(partido);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Partidos/Index",
                new { message = "Partido creado exitosamente." });
        }

        private async Task CargarDatosAsync()
        {
            Torneos = await _context.Torneos
                .Where(t => t.Estado != "Finalizado")
                .OrderByDescending(t => t.FechaInicio)
                .Select(t => new TorneoDropdownItem { Id = t.Id, Nombre = t.Nombre })
                .ToListAsync();

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

            // Build equipo map per torneo for JS filtering
            var equiposTorneos = await _context.EquiposTorneos
                .Include(et => et.Equipo)
                .Where(et => et.Torneo.Estado != "Finalizado")
                .Select(et => new { et.TorneoId, et.EquipoId, et.Equipo.Nombre })
                .ToListAsync();

            var map = equiposTorneos
                .GroupBy(et => et.TorneoId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => new { id = e.EquipoId, nombre = e.Nombre })
                          .OrderBy(e => e.nombre)
                          .ToList());

            EquiposPorTorneoJson = JsonSerializer.Serialize(map);
        }
    }

    public class TorneoDropdownItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class ArbitroDropdownItem
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
    }

    public class CrearPartidoInput
    {
        [Required(ErrorMessage = "Seleccione un torneo")]
        [Display(Name = "Torneo")]
        public int TorneoId { get; set; }

        [Required(ErrorMessage = "Seleccione el equipo local")]
        [Display(Name = "Equipo Local")]
        public int EquipoLocalId { get; set; }

        [Required(ErrorMessage = "Seleccione el equipo visitante")]
        [Display(Name = "Equipo Visitante")]
        public int EquipoVisitanteId { get; set; }

        [Display(Name = "Árbitro")]
        public int? ArbitroId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha y Hora")]
        public DateTime Fecha { get; set; } = DateTime.Today.AddDays(7);

        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        [Display(Name = "Sede")]
        public string? Sede { get; set; }
    }
}
