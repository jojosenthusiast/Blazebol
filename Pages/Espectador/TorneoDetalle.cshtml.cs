using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Espectador
{
    public class TorneoDetalleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TorneoDetalleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Torneo Torneo { get; set; } = null!;
        public ConfiguracionTorneo? Config { get; set; }
        public List<ClasificacionViewModel> Clasificacion { get; set; } = new();
        public List<GrupoFaseViewModel> Grupos { get; set; } = new();
        public List<PartidoResumenViewModel> PartidosFaseElim { get; set; } = new();

        // Modo Finalizado
        public bool EsFinalizado { get; set; }
        public ClasificacionViewModel? Campeon { get; set; }
        public List<ClasificacionViewModel> TablaFinal { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Espectador"))
                return RedirectToPage("/Account/Login");

            var torneo = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .Include(t => t.ConfiguracionTorneo)
                .Include(t => t.EquiposTorneos).ThenInclude(et => et.Equipo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (torneo == null)
            {
                ErrorMessage = "Torneo no encontrado.";
                return Page();
            }

            // Borradores son invisibles para el espectador
            if (torneo.Estado == EstadosTorneo.Borrador)
                return RedirectToPage("/Espectador/Torneos");

            Torneo = torneo;
            Config = torneo.ConfiguracionTorneo;

            var partidos = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Grupo)
                .Where(p => p.TorneoId == id && p.Estado != EstadosPartido.Cancelado)
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            // Modo Finalizado — solo mostrar campeón + tabla de honor
            if (torneo.Estado == EstadosTorneo.Finalizado)
            {
                EsFinalizado = true;

                if (torneo.Formato == FormatosTorneo.TodosContraTodos)
                {
                    TablaFinal = CalcularClasificacion(
                        torneo.EquiposTorneos.Select(et => et.Equipo).ToList(),
                        partidos, Config);
                }
                else
                {
                    // Eliminación directa o Grupos+Eliminación: campeón = ganador del último partido finalizado
                    var finalPartido = partidos
                        .Where(p => p.Estado == EstadosPartido.Finalizado)
                        .OrderByDescending(p => p.Fecha)
                        .FirstOrDefault();

                    if (finalPartido != null)
                    {
                        bool localGana = (finalPartido.GolesLocal ?? 0) >= (finalPartido.GolesVisitante ?? 0);
                        var campeonEquipo = localGana ? finalPartido.EquipoLocal : finalPartido.EquipoVisitante;
                        var subcampeonEquipo = localGana ? finalPartido.EquipoVisitante : finalPartido.EquipoLocal;

                        TablaFinal = new List<ClasificacionViewModel>
                        {
                            new() { Posicion = 1, EquipoId = campeonEquipo.Id, NombreEquipo = campeonEquipo.Nombre,
                                    Abreviacion = campeonEquipo.Abreviacion, Logo = campeonEquipo.Logo,
                                    ColorPrimario = campeonEquipo.ColorPrimario },
                            new() { Posicion = 2, EquipoId = subcampeonEquipo.Id, NombreEquipo = subcampeonEquipo.Nombre,
                                    Abreviacion = subcampeonEquipo.Abreviacion, Logo = subcampeonEquipo.Logo,
                                    ColorPrimario = subcampeonEquipo.ColorPrimario }
                        };
                    }
                    else
                    {
                        // sin partidos finalizados: listar todos los equipos
                        TablaFinal = torneo.EquiposTorneos.Select((et, i) => new ClasificacionViewModel
                        {
                            Posicion = i + 1, EquipoId = et.Equipo.Id, NombreEquipo = et.Equipo.Nombre,
                            Abreviacion = et.Equipo.Abreviacion, Logo = et.Equipo.Logo,
                            ColorPrimario = et.Equipo.ColorPrimario
                        }).ToList();
                    }
                }

                Campeon = TablaFinal.FirstOrDefault();
                return Page();
            }

            // Torneo Activo — lógica normal
            if (torneo.Formato == FormatosTorneo.TodosContraTodos)
            {
                Clasificacion = CalcularClasificacion(
                    torneo.EquiposTorneos.Select(et => et.Equipo).ToList(),
                    partidos, Config);
            }
            else if (torneo.Formato == FormatosTorneo.GruposYEliminacion)
            {
                var grupos = await _context.Set<Grupo>()
                    .Where(g => g.TorneoId == id)
                    .OrderBy(g => g.Nombre)
                    .ToListAsync();

                foreach (var grupo in grupos)
                {
                    var partidosGrupo = partidos.Where(p => p.GrupoId == grupo.Id).ToList();
                    var equiposEnGrupo = partidosGrupo
                        .SelectMany(p => new[] { p.EquipoLocal, p.EquipoVisitante })
                        .DistinctBy(e => e.Id)
                        .ToList();

                    Grupos.Add(new GrupoFaseViewModel
                    {
                        NombreGrupo = grupo.Nombre,
                        Clasificacion = CalcularClasificacion(equiposEnGrupo, partidosGrupo, Config),
                        Partidos = Map(partidosGrupo)
                    });
                }

                PartidosFaseElim = Map(partidos.Where(p => p.GrupoId == null).ToList());
            }
            else // EliminacionDirecta
            {
                PartidosFaseElim = Map(partidos);
            }

            return Page();
        }

        private static List<PartidoResumenViewModel> Map(List<Partido> partidos) =>
            partidos.Select(p => new PartidoResumenViewModel
            {
                Id = p.Id,
                Fecha = p.Fecha,
                NombreLocal = p.EquipoLocal.Nombre,
                AbrevLocal = p.EquipoLocal.Abreviacion,
                LogoLocal = p.EquipoLocal.Logo,
                ColorLocal = p.EquipoLocal.ColorPrimario,
                NombreVisitante = p.EquipoVisitante.Nombre,
                AbrevVisitante = p.EquipoVisitante.Abreviacion,
                LogoVisitante = p.EquipoVisitante.Logo,
                ColorVisitante = p.EquipoVisitante.ColorPrimario,
                GolesLocal = p.GolesLocal,
                GolesVisitante = p.GolesVisitante,
                Estado = p.Estado,
                Sede = p.Sede
            }).ToList();

        private static List<ClasificacionViewModel> CalcularClasificacion(
            List<Equipo> equipos, List<Partido> partidos, ConfiguracionTorneo? config)
        {
            int ptsVic = config?.PuntosPorVictoria ?? 3;
            int ptsEmp = config?.PuntosPorEmpate ?? 1;

            return equipos.Select(e =>
            {
                var local = partidos.Where(p => p.EquipoLocalId == e.Id && p.Estado == EstadosPartido.Finalizado).ToList();
                var visita = partidos.Where(p => p.EquipoVisitanteId == e.Id && p.Estado == EstadosPartido.Finalizado).ToList();

                int gf = local.Sum(p => p.GolesLocal ?? 0) + visita.Sum(p => p.GolesVisitante ?? 0);
                int gc = local.Sum(p => p.GolesVisitante ?? 0) + visita.Sum(p => p.GolesLocal ?? 0);
                int pg = local.Count(p => (p.GolesLocal ?? 0) > (p.GolesVisitante ?? 0)) +
                         visita.Count(p => (p.GolesVisitante ?? 0) > (p.GolesLocal ?? 0));
                int pe = local.Count(p => p.GolesLocal == p.GolesVisitante) +
                         visita.Count(p => p.GolesLocal == p.GolesVisitante);
                int pp = (local.Count + visita.Count) - pg - pe;

                return new ClasificacionViewModel
                {
                    EquipoId = e.Id,
                    NombreEquipo = e.Nombre,
                    Abreviacion = e.Abreviacion,
                    Logo = e.Logo,
                    ColorPrimario = e.ColorPrimario,
                    PJ = local.Count + visita.Count,
                    PG = pg, PE = pe, PP = pp,
                    GF = gf, GC = gc,
                    Pts = pg * ptsVic + pe * ptsEmp
                };
            })
            .OrderByDescending(c => c.Pts)
            .ThenByDescending(c => c.GF - c.GC)
            .ThenByDescending(c => c.GF)
            .Select((c, i) => { c.Posicion = i + 1; return c; })
            .ToList();
        }
    }

    public class PartidoResumenViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string? AbrevLocal { get; set; }
        public string? LogoLocal { get; set; }
        public string? ColorLocal { get; set; }
        public string NombreVisitante { get; set; } = string.Empty;
        public string? AbrevVisitante { get; set; }
        public string? LogoVisitante { get; set; }
        public string? ColorVisitante { get; set; }
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Sede { get; set; }

        public string Resultado => Estado == EstadosPartido.Finalizado
            ? $"{GolesLocal} - {GolesVisitante}"
            : Estado == EstadosPartido.EnCurso ? "En Curso" : "—";
    }

    public class ClasificacionViewModel
    {
        public int Posicion { get; set; }
        public int EquipoId { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? ColorPrimario { get; set; }
        public int PJ { get; set; }
        public int PG { get; set; }
        public int PE { get; set; }
        public int PP { get; set; }
        public int GF { get; set; }
        public int GC { get; set; }
        public int Pts { get; set; }
        public int DG => GF - GC;
    }

    public class GrupoFaseViewModel
    {
        public string NombreGrupo { get; set; } = string.Empty;
        public List<ClasificacionViewModel> Clasificacion { get; set; } = new();
        public List<PartidoResumenViewModel> Partidos { get; set; } = new();
    }
}
