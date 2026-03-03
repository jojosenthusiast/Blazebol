using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Arbitro.Partidos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PartidoArbitroViewModel> Partidos { get; set; } = new();
        public string EstadoAprobacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Arbitro)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Arbitro == null)
                return RedirectToPage("/Error");

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";
            EstadoAprobacion = persona.Arbitro.EstadoAprobacion;

            if (EstadoAprobacion != EstadosAprobacionArbitro.Aprobado)
                return Page();

            Partidos = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Where(p => p.ArbitroId == persona.Arbitro.Id)
                .OrderBy(p => p.Fecha)
                .Select(p => new PartidoArbitroViewModel
                {
                    Id = p.Id,
                    Fecha = p.Fecha,
                    NombreTorneo = p.Torneo.Nombre,
                    NombreLocal = p.EquipoLocal.Nombre,
                    NombreVisitante = p.EquipoVisitante.Nombre,
                    Estado = p.Estado,
                    Sede = p.Sede,
                    GolesLocal = p.GolesLocal,
                    GolesVisitante = p.GolesVisitante
                })
                .ToListAsync();

            return Page();
        }
    }

    public class PartidoArbitroViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Sede { get; set; }
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }

        public bool EsProgramado => Estado == EstadosPartido.Programado;
        public string Resultado => Estado == EstadosPartido.Finalizado && GolesLocal.HasValue && GolesVisitante.HasValue
            ? $"{GolesLocal} - {GolesVisitante}"
            : "-";
    }
}
