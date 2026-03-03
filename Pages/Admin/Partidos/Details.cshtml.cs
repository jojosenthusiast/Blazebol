using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Partidos
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PartidoDetalleViewModel Partido { get; set; } = null!;
        public List<JugadorLineupViewModel> JugadoresLocal { get; set; } = new();
        public List<JugadorLineupViewModel> JugadoresVisitante { get; set; } = new();
        public List<ClasificacionViewModel> Clasificacion { get; set; } = new();
        public string FormatoTorneo { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var partido = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Arbitro).ThenInclude(a => a!.Persona)
                .Include(p => p.Grupo)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partido == null) return NotFound();

            FormatoTorneo = partido.Torneo.Formato;

            Partido = new PartidoDetalleViewModel
            {
                Id = partido.Id,
                TorneoId = partido.TorneoId,
                NombreTorneo = partido.Torneo.Nombre,
                FormatoTorneo = partido.Torneo.Formato,
                NombreGrupo = partido.Grupo?.Nombre,
                NombreLocal = partido.EquipoLocal.Nombre,
                NombreVisitante = partido.EquipoVisitante.Nombre,
                EquipoLocalId = partido.EquipoLocalId,
                EquipoVisitanteId = partido.EquipoVisitanteId,
                Fecha = partido.Fecha,
                Sede = partido.Sede,
                Estado = partido.Estado,
                GolesLocal = partido.GolesLocal,
                GolesVisitante = partido.GolesVisitante,
                NombreArbitro = partido.Arbitro != null
                    ? $"{partido.Arbitro.Persona.Nombre} {partido.Arbitro.Persona.ApellidoPaterno}"
                    : null
            };

            // Jugadores del equipo local inscritos en el torneo
            JugadoresLocal = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoLocalId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new JugadorLineupViewModel
                {
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    NombreCompleto = ejt.Jugador.Persona.Nombre + " " + ejt.Jugador.Persona.ApellidoPaterno,
                    Posicion = ejt.Jugador.PosicionPreferida ?? "N/A"
                })
                .ToListAsync();

            // Jugadores del equipo visitante inscritos en el torneo
            JugadoresVisitante = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoVisitanteId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new JugadorLineupViewModel
                {
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    NombreCompleto = ejt.Jugador.Persona.Nombre + " " + ejt.Jugador.Persona.ApellidoPaterno,
                    Posicion = ejt.Jugador.PosicionPreferida ?? "N/A"
                })
                .ToListAsync();

            // Clasificación: todos los partidos finalizados del torneo
            await CalcularClasificacionAsync(partido.TorneoId);

            return Page();
        }

        private async Task CalcularClasificacionAsync(int torneoId)
        {
            // Equipos inscritos en el torneo
            var equiposInscritos = await _context.EquiposTorneos
                .Include(et => et.Equipo)
                .Where(et => et.TorneoId == torneoId)
                .ToListAsync();

            // Partidos finalizados del torneo
            var partidos = await _context.Partidos
                .Where(p => p.TorneoId == torneoId && p.Estado == EstadosPartido.Finalizado
                         && p.GolesLocal.HasValue && p.GolesVisitante.HasValue)
                .ToListAsync();

            // Configuración de puntos
            var config = await _context.ConfiguracionesTorneo
                .FirstOrDefaultAsync(c => c.TorneoId == torneoId);
            int ptsVic = config?.PuntosPorVictoria ?? 3;
            int ptsEmp = config?.PuntosPorEmpate ?? 1;
            int ptsDer = config?.PuntosPorDerrota ?? 0;

            // Calcular estadísticas por equipo
            var stats = equiposInscritos.ToDictionary(
                et => et.EquipoId,
                et => new ClasificacionViewModel
                {
                    NombreEquipo = et.Equipo.Nombre,
                    EquipoId = et.EquipoId
                });

            foreach (var p in partidos)
            {
                if (!stats.ContainsKey(p.EquipoLocalId) || !stats.ContainsKey(p.EquipoVisitanteId))
                    continue;

                var local = stats[p.EquipoLocalId];
                var visit = stats[p.EquipoVisitanteId];
                int gl = p.GolesLocal!.Value;
                int gv = p.GolesVisitante!.Value;

                local.PJ++;
                visit.PJ++;
                local.GF += gl;
                local.GC += gv;
                visit.GF += gv;
                visit.GC += gl;

                if (gl > gv) { local.PG++; visit.PP++; local.Pts += ptsVic; visit.Pts += ptsDer; }
                else if (gl < gv) { visit.PG++; local.PP++; visit.Pts += ptsVic; local.Pts += ptsDer; }
                else { local.PE++; visit.PE++; local.Pts += ptsEmp; visit.Pts += ptsEmp; }
            }

            Clasificacion = stats.Values
                .OrderByDescending(s => s.Pts)
                .ThenByDescending(s => s.DG)
                .ThenByDescending(s => s.GF)
                .ThenBy(s => s.NombreEquipo)
                .ToList();

            // Asignar posiciones
            for (int i = 0; i < Clasificacion.Count; i++)
                Clasificacion[i].Posicion = i + 1;
        }
    }

    public class PartidoDetalleViewModel
    {
        public int Id { get; set; }
        public int TorneoId { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public string FormatoTorneo { get; set; } = string.Empty;
        public string? NombreGrupo { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public int EquipoLocalId { get; set; }
        public int EquipoVisitanteId { get; set; }
        public DateTime Fecha { get; set; }
        public string? Sede { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public string? NombreArbitro { get; set; }

        public string Resultado => Estado == EstadosPartido.Finalizado && GolesLocal.HasValue && GolesVisitante.HasValue
            ? $"{GolesLocal} - {GolesVisitante}"
            : "vs";

        public string? GanadorNombre => Estado == EstadosPartido.Finalizado && GolesLocal.HasValue && GolesVisitante.HasValue
            ? GolesLocal > GolesVisitante ? NombreLocal
            : GolesVisitante > GolesLocal ? NombreVisitante
            : null
            : null;
    }

    public class JugadorLineupViewModel
    {
        public int NumeroCamiseta { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Posicion { get; set; } = string.Empty;
    }

    public class ClasificacionViewModel
    {
        public int Posicion { get; set; }
        public int EquipoId { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public int PJ { get; set; }
        public int PG { get; set; }
        public int PE { get; set; }
        public int PP { get; set; }
        public int GF { get; set; }
        public int GC { get; set; }
        public int DG => GF - GC;
        public int Pts { get; set; }
    }
}
