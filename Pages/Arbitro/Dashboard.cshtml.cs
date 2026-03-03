using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Arbitro
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoAprobacion { get; set; } = string.Empty;
        public int PartidosArbitrados { get; set; }
        public List<ProximoPartidoViewModel> ProximosPartidos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return RedirectToPage("/Account/Login");

            if (!HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Error", new { statusCode = 403 });

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

            if (EstadoAprobacion == EstadosAprobacionArbitro.Aprobado)
            {
                var arbitroId = persona.Arbitro.Id;

                PartidosArbitrados = await _context.Partidos
                    .CountAsync(p => p.ArbitroId == arbitroId
                               && p.Estado == EstadosPartido.Finalizado);

                ProximosPartidos = await _context.Partidos
                    .Include(p => p.Torneo)
                    .Include(p => p.EquipoLocal)
                    .Include(p => p.EquipoVisitante)
                    .Where(p => p.ArbitroId == arbitroId
                             && p.Estado == EstadosPartido.Programado
                             && p.Fecha >= DateTime.UtcNow)
                    .OrderBy(p => p.Fecha)
                    .Take(5)
                    .Select(p => new ProximoPartidoViewModel
                    {
                        Id = p.Id,
                        Fecha = p.Fecha,
                        NombreTorneo = p.Torneo.Nombre,
                        NombreLocal = p.EquipoLocal.Nombre,
                        NombreVisitante = p.EquipoVisitante.Nombre,
                        Sede = p.Sede
                    })
                    .ToListAsync();
            }

            return Page();
        }
    }

    public class ProximoPartidoViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string? Sede { get; set; }
    }
}
