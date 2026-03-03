using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Partidos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PartidoItemViewModel> Partidos { get; set; } = new();
        public string? FiltroEstado { get; set; }
        public int? FiltroTorneoId { get; set; }
        public Dictionary<int, string> TorneosDisponibles { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? estado, int? torneoId, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            FiltroEstado = estado;
            FiltroTorneoId = torneoId;

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            var query = _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Arbitro).ThenInclude(a => a.Persona)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(p => p.Estado == estado);

            if (torneoId.HasValue)
                query = query.Where(p => p.TorneoId == torneoId.Value);

            var partidos = await query
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            Partidos = partidos.Select(p => new PartidoItemViewModel
            {
                Id = p.Id,
                Fecha = p.Fecha,
                NombreTorneo = p.Torneo.Nombre,
                NombreLocal = p.EquipoLocal.Nombre,
                NombreVisitante = p.EquipoVisitante.Nombre,
                NombreArbitro = p.Arbitro != null
                    ? $"{p.Arbitro.Persona.Nombre} {p.Arbitro.Persona.ApellidoPaterno}"
                    : null,
                Estado = p.Estado,
                GolesLocal = p.GolesLocal,
                GolesVisitante = p.GolesVisitante,
                Sede = p.Sede
            }).ToList();

            TorneosDisponibles = await _context.Torneos
                .OrderByDescending(t => t.FechaInicio)
                .ToDictionaryAsync(t => t.Id, t => t.Nombre);

            return Page();
        }
    }

    public class PartidoItemViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string? NombreArbitro { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public string? Sede { get; set; }

        public string Resultado => Estado == "Finalizado" && GolesLocal.HasValue && GolesVisitante.HasValue
            ? $"{GolesLocal} - {GolesVisitante}"
            : "-";
    }
}
