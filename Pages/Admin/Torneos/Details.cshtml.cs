using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IFixtureService _fixture;

        public DetailsModel(ApplicationDbContext context, IFixtureService fixture)
        {
            _context = context;
            _fixture = fixture;
        }

        public TorneoItemViewModel Torneo { get; set; } = new();
        public ConfiguracionTorneo? Configuracion { get; set; }
        public List<ReglaPersonalizadaViewModel> ReglasPersonalizadas { get; set; } = new();
        public List<EquipoInscritoViewModel> EquiposInscritos { get; set; } = new();
        public bool PuedeEditar { get; set; }
        public string? SuccessMessage { get; set; }

        // Resumen para torneos Finalizados
        public string? NombreCampeon { get; set; }
        public string? LogoCampeon { get; set; }
        public string? ColorCampeon { get; set; }
        public string? AbreviacionCampeon { get; set; }
        public int TotalPartidosJugados { get; set; }
        public int TotalGolesAnotados { get; set; }

        // Fixture status
        public string? UrlGrupoWhatsApp { get; set; }
        public bool TieneFixture { get; set; }
        public int FixtureTotalPartidos { get; set; }
        public int FixtureProgramados { get; set; }
        public int FixtureJugados { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var torneo = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .Include(t => t.ConfiguracionTorneo)
                .Include(t => t.EquiposTorneos).ThenInclude(et => et.Equipo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (torneo == null)
                return NotFound();

            Torneo = new TorneoItemViewModel
            {
                Id = torneo.Id,
                Nombre = torneo.Nombre,
                NombreLiga = torneo.Liga.Nombre,
                NombreTemporada = torneo.Temporada.Nombre,
                NombreCategoria = torneo.Categoria.Nombre,
                Formato = torneo.Formato,
                Estado = torneo.Estado,
                FechaInicio = torneo.FechaInicio,
                FechaFin = torneo.FechaFin,
                NumeroEquiposMinimo = torneo.NumeroEquiposMinimo,
                NumeroEquiposMaximo = torneo.NumeroEquiposMaximo,
                NumeroEquiposInscritos = torneo.EquiposTorneos.Count,
                PuedeEditar = torneo.Estado != EstadosTorneo.Finalizado
            };

            Configuracion = torneo.ConfiguracionTorneo;
            PuedeEditar = torneo.Estado != EstadosTorneo.Finalizado;
            UrlGrupoWhatsApp = torneo.UrlGrupoWhatsApp;

            EquiposInscritos = torneo.EquiposTorneos.Select(et => new EquipoInscritoViewModel
            {
                Id = et.Equipo.Id,
                Nombre = et.Equipo.Nombre,
                Abreviacion = et.Equipo.Abreviacion,
                Logo = et.Equipo.Logo,
                ColorPrimario = et.Equipo.ColorPrimario
            }).ToList();

            // Fixture stats
            TieneFixture = await _fixture.TieneFixture(id);
            if (TieneFixture)
            {
                var (total, programados, jugados) = await _fixture.EstadisticasFixture(id);
                FixtureTotalPartidos = total;
                FixtureProgramados = programados;
                FixtureJugados = jugados;
            }

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            try
            {
                ReglasPersonalizadas = await _context.ReglasPersonalizadasTorneo
                    .Where(r => r.TorneoId == id)
                    .OrderBy(r => r.Orden)
                    .Select(r => new ReglaPersonalizadaViewModel
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion,
                        Tipo = r.Tipo,
                        Activa = r.Activa,
                        EsAutomatica = r.EsAutomatica
                    })
                    .ToListAsync();
            }
            catch
            {
                ReglasPersonalizadas = new();
            }

            // Resumen de resultados para torneos Finalizados
            if (torneo.Estado == EstadosTorneo.Finalizado)
            {
                var partidos = await _context.Partidos
                    .Include(p => p.EquipoLocal)
                    .Include(p => p.EquipoVisitante)
                    .Where(p => p.TorneoId == id && p.Estado == EstadosPartido.Finalizado)
                    .ToListAsync();

                TotalPartidosJugados = partidos.Count;
                TotalGolesAnotados = partidos.Sum(p => (p.GolesLocal ?? 0) + (p.GolesVisitante ?? 0));

                Equipo? campeonEquipo = null;

                if (torneo.Formato == FormatosTorneo.TodosContraTodos)
                {
                    // Campeon = equipo con más puntos
                    var config = torneo.ConfiguracionTorneo;
                    int ptsV = config?.PuntosPorVictoria ?? 3;
                    int ptsE = config?.PuntosPorEmpate ?? 1;

                    var equipos = torneo.EquiposTorneos.Select(et => et.Equipo).ToList();
                    var topEquipo = equipos.Select(e =>
                    {
                        var localP = partidos.Where(p => p.EquipoLocalId == e.Id).ToList();
                        var visitP = partidos.Where(p => p.EquipoVisitanteId == e.Id).ToList();
                        int pg = localP.Count(p => (p.GolesLocal ?? 0) > (p.GolesVisitante ?? 0)) +
                                 visitP.Count(p => (p.GolesVisitante ?? 0) > (p.GolesLocal ?? 0));
                        int pe = localP.Count(p => p.GolesLocal == p.GolesVisitante) +
                                 visitP.Count(p => p.GolesLocal == p.GolesVisitante);
                        int gf = localP.Sum(p => p.GolesLocal ?? 0) + visitP.Sum(p => p.GolesVisitante ?? 0);
                        int dg = gf - (localP.Sum(p => p.GolesVisitante ?? 0) + visitP.Sum(p => p.GolesLocal ?? 0));
                        return new { Equipo = e, Pts = pg * ptsV + pe * ptsE, DG = dg, GF = gf };
                    })
                    .OrderByDescending(x => x.Pts)
                    .ThenByDescending(x => x.DG)
                    .ThenByDescending(x => x.GF)
                    .FirstOrDefault();

                    campeonEquipo = topEquipo?.Equipo;
                }
                else
                {
                    // Para eliminación: ganador del último partido finalizado
                    var final = partidos.OrderByDescending(p => p.Fecha).FirstOrDefault();
                    if (final != null)
                    {
                        bool localGana = (final.GolesLocal ?? 0) >= (final.GolesVisitante ?? 0);
                        campeonEquipo = localGana ? final.EquipoLocal : final.EquipoVisitante;
                    }
                }

                if (campeonEquipo != null)
                {
                    NombreCampeon = campeonEquipo.Nombre;
                    LogoCampeon = campeonEquipo.Logo;
                    ColorCampeon = campeonEquipo.ColorPrimario;
                    AbreviacionCampeon = campeonEquipo.Abreviacion;
                }
            }

            return Page();
        }
    }

    public class EquipoInscritoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? ColorPrimario { get; set; }
    }
}
