using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Entrenador.Partidos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreEquipo { get; set; } = string.Empty;
        public int? EquipoId { get; set; }
        public List<PartidoEntrenadorViewModel> PartidosEnCurso { get; set; } = new();
        public List<PartidoEntrenadorViewModel> ProximosPartidos { get; set; } = new();
        public List<PartidoEntrenadorViewModel> PartidosPasados { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                    .ThenInclude(e => e!.EquiposEntrenadores.Where(ee => ee.Activo && ee.EsPropietario))
                    .ThenInclude(ee => ee.Equipo)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Entrenador == null) return RedirectToPage("/Error");

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";

            var equipoEntrenador = persona.Entrenador.EquiposEntrenadores.FirstOrDefault();
            if (equipoEntrenador == null) return Page();

            EquipoId = equipoEntrenador.EquipoId;
            NombreEquipo = equipoEntrenador.Equipo.Nombre;

            var partidos = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Arbitro).ThenInclude(a => a!.Persona)
                .Include(p => p.Grupo)
                .Where(p => (p.EquipoLocalId == EquipoId || p.EquipoVisitanteId == EquipoId)
                         && p.Estado != EstadosPartido.Cancelado)
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            var mapped = partidos.Select(p => new PartidoEntrenadorViewModel
            {
                Id = p.Id,
                NombreTorneo = p.Torneo.Nombre,
                NombreGrupo = p.Grupo?.Nombre,
                NombreLocal = p.EquipoLocal.Nombre,
                AbrevLocal = p.EquipoLocal.Abreviacion,
                ColorLocal = p.EquipoLocal.ColorPrimario,
                LogoLocal = p.EquipoLocal.Logo,
                NombreVisitante = p.EquipoVisitante.Nombre,
                AbrevVisitante = p.EquipoVisitante.Abreviacion,
                ColorVisitante = p.EquipoVisitante.ColorPrimario,
                LogoVisitante = p.EquipoVisitante.Logo,
                Fecha = p.Fecha,
                Sede = p.Sede,
                Estado = p.Estado,
                GolesLocal = p.GolesLocal,
                GolesVisitante = p.GolesVisitante,
                NombreArbitro = p.Arbitro != null
                    ? $"{p.Arbitro.Persona.Nombre} {p.Arbitro.Persona.ApellidoPaterno}" : null,
                EsMiEquipoLocal = p.EquipoLocalId == EquipoId,
                TorneoId = p.TorneoId
            }).ToList();

            PartidosEnCurso = mapped.Where(p => p.Estado == EstadosPartido.EnCurso).ToList();
            ProximosPartidos = mapped.Where(p => p.Estado == EstadosPartido.Programado).ToList();
            PartidosPasados = mapped.Where(p => p.Estado == EstadosPartido.Finalizado)
                                    .OrderByDescending(p => p.Fecha).ToList();

            return Page();
        }
    }

    public class PartidoEntrenadorViewModel
    {
        public int Id { get; set; }
        public int TorneoId { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string? NombreGrupo { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string? AbrevLocal { get; set; }
        public string? ColorLocal { get; set; }
        public string? LogoLocal { get; set; }
        public string NombreVisitante { get; set; } = string.Empty;
        public string? AbrevVisitante { get; set; }
        public string? ColorVisitante { get; set; }
        public string? LogoVisitante { get; set; }
        public DateTime Fecha { get; set; }
        public string? Sede { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public string? NombreArbitro { get; set; }
        public bool EsMiEquipoLocal { get; set; }
    }
}
