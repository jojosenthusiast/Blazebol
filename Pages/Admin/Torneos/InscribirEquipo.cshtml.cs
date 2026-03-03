using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class InscribirEquipoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InscribirEquipoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TorneoId { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string EstadoTorneo { get; set; } = string.Empty;
        public int? MaxEquipos { get; set; }
        public string? BuscarTerm { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public List<EquipoInscripcionViewModel> EquiposDisponibles { get; set; } = new();
        public List<EquipoInscripcionViewModel> EquiposInscritos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int torneoId, string? buscar, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            var torneo = await _context.Torneos.FindAsync(torneoId);
            if (torneo == null) return NotFound();

            TorneoId = torneoId;
            NombreTorneo = torneo.Nombre;
            EstadoTorneo = torneo.Estado;
            MaxEquipos = torneo.NumeroEquiposMaximo;
            BuscarTerm = buscar;

            var inscritosIds = await _context.EquiposTorneos
                .Where(et => et.TorneoId == torneoId)
                .Select(et => et.EquipoId)
                .ToListAsync();

            EquiposInscritos = await _context.EquiposTorneos
                .Include(et => et.Equipo)
                .Where(et => et.TorneoId == torneoId)
                .Select(et => new EquipoInscripcionViewModel
                {
                    EquipoId = et.EquipoId,
                    Nombre = et.Equipo.Nombre,
                    Sede = et.Equipo.Sede,
                    FechaInscripcion = et.FechaInscripcion
                })
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            var query = _context.Equipos
                .Where(e => e.Activo && !inscritosIds.Contains(e.Id));

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(e => e.Nombre.ToLower().Contains(buscar.ToLower()));

            EquiposDisponibles = await query
                .Select(e => new EquipoInscripcionViewModel
                {
                    EquipoId = e.Id,
                    Nombre = e.Nombre,
                    Sede = e.Sede
                })
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int torneoId, int equipoId)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var torneo = await _context.Torneos.FindAsync(torneoId);
            if (torneo == null) return NotFound();

            if (torneo.Estado == "Finalizado")
            {
                ErrorMessage = "No se pueden inscribir equipos en un torneo finalizado.";
                return await OnGetAsync(torneoId, null, null);
            }

            // Check max teams
            if (torneo.NumeroEquiposMaximo.HasValue)
            {
                var count = await _context.EquiposTorneos.CountAsync(et => et.TorneoId == torneoId);
                if (count >= torneo.NumeroEquiposMaximo.Value)
                {
                    ErrorMessage = $"El torneo ya alcanzó el máximo de {torneo.NumeroEquiposMaximo} equipos.";
                    return await OnGetAsync(torneoId, null, null);
                }
            }

            // Check not already inscribed
            if (await _context.EquiposTorneos.AnyAsync(et => et.TorneoId == torneoId && et.EquipoId == equipoId))
            {
                ErrorMessage = "El equipo ya está inscrito en este torneo.";
                return await OnGetAsync(torneoId, null, null);
            }

            _context.EquiposTorneos.Add(new EquipoTorneo
            {
                EquipoId = equipoId,
                TorneoId = torneoId,
                FechaInscripcion = DateTime.UtcNow,
                Estado = "Activo"
            });
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Torneos/Details",
                new { id = torneoId, message = "Equipo inscrito exitosamente." });
        }
    }

    public class EquipoInscripcionViewModel
    {
        public int EquipoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Sede { get; set; }
        public DateTime? FechaInscripcion { get; set; }
    }
}
