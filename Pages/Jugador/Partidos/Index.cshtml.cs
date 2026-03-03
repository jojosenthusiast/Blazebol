using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Jugador.Partidos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreCompleto { get; set; } = string.Empty;
        public string? NombreEquipo { get; set; }
        public List<PartidoJugadorViewModel> PartidosEnCurso { get; set; } = new();
        public List<PartidoJugadorViewModel> ProximosPartidos { get; set; } = new();
        public List<PartidoJugadorViewModel> PartidosPasados { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Jugador"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Jugador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Jugador == null) return RedirectToPage("/Error");

            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno}";
            var jugadorId = persona.Jugador.Id;

            // Teams where this jugador is active
            var inscripciones = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Equipo)
                .Where(ejt => ejt.JugadorId == jugadorId && ejt.Activo)
                .ToListAsync();

            var equipoIds = inscripciones.Select(e => e.EquipoId).Distinct().ToList();

            // Use the most recent team name
            NombreEquipo = inscripciones
                .OrderByDescending(e => e.FechaInscripcion)
                .FirstOrDefault()?.Equipo?.Nombre;

            if (!equipoIds.Any()) return Page();

            var partidos = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Arbitro).ThenInclude(a => a!.Persona)
                .Where(p => (equipoIds.Contains(p.EquipoLocalId) || equipoIds.Contains(p.EquipoVisitanteId))
                         && p.Estado != EstadosPartido.Cancelado)
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            var mapped = partidos.Select(p => new PartidoJugadorViewModel
            {
                Id = p.Id,
                NombreTorneo = p.Torneo.Nombre,
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
                EsMiEquipoLocal = equipoIds.Contains(p.EquipoLocalId)
            }).ToList();

            PartidosEnCurso = mapped.Where(p => p.Estado == EstadosPartido.EnCurso).ToList();
            ProximosPartidos = mapped.Where(p => p.Estado == EstadosPartido.Programado).ToList();
            PartidosPasados = mapped.Where(p => p.Estado == EstadosPartido.Finalizado)
                                    .OrderByDescending(p => p.Fecha).ToList();

            return Page();
        }
    }

    public class PartidoJugadorViewModel
    {
        public int Id { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
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
