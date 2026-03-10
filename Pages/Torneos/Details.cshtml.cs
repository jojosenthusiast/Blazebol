using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Models.ViewModels;

namespace ControlTorneosFutbol.Pages.Torneos
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public TorneoDetallePublicoViewModel Torneo { get; set; } = null!;
        public bool IsAuthenticated { get; set; }
        public string DashboardPage { get; set; } = "/Account/Login";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            IsAuthenticated = HttpContext.Session.IsAuthenticated();
            var role = HttpContext.Session.GetUserRole();
            if (IsAuthenticated && !string.IsNullOrEmpty(role))
            {
                DashboardPage = role switch
                {
                    "Administrador" => "/Admin/Dashboard",
                    "Jugador"       => "/Jugador/Dashboard",
                    "Entrenador"    => "/Entrenador/Dashboard",
                    "Arbitro"       => "/Arbitro/Dashboard",
                    "Espectador"    => "/Espectador/Index",
                    _               => "/Account/Login"
                };
            }

            var torneo = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Categoria)
                .Include(t => t.ConfiguracionTorneo)
                .Include(t => t.EquiposTorneos)
                    .ThenInclude(et => et.Equipo)
                        .ThenInclude(e => e.EquiposEntrenadores)
                            .ThenInclude(ee => ee.Entrenador)
                                .ThenInclude(en => en.Persona)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (torneo == null)
                return NotFound();

            // Parsear galería y patrocinadores (JSON arrays)
            var galeria = new List<string>();
            if (!string.IsNullOrEmpty(torneo.ImagenesGaleria))
            {
                try { galeria = JsonSerializer.Deserialize<List<string>>(torneo.ImagenesGaleria) ?? new(); }
                catch { /* malformed json */ }
            }

            var patrocinadores = new List<string>();
            if (!string.IsNullOrEmpty(torneo.Patrocinadores))
            {
                try { patrocinadores = JsonSerializer.Deserialize<List<string>>(torneo.Patrocinadores) ?? new(); }
                catch { /* malformed json */ }
            }

            // Equipos inscritos con entrenador
            var equipos = torneo.EquiposTorneos.Select(et =>
            {
                var e = et.Equipo;
                var ee = e.EquiposEntrenadores.FirstOrDefault();
                string? entrenadorNombre = null;
                if (ee != null)
                    entrenadorNombre = $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}";

                return new EquipoInscritoViewModel
                {
                    Id             = e.Id,
                    Nombre         = e.Nombre,
                    Abreviacion    = e.Abreviacion,
                    Logo           = e.Logo,
                    EntrenadorNombre = entrenadorNombre
                };
            }).ToList();

            var todosPartidos = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Where(p => p.TorneoId == id)
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            var partidosVM = todosPartidos.Select(p => new PartidoPublicoViewModel
            {
                Id                      = p.Id,
                Fecha                   = p.Fecha,
                Sede                    = p.Sede,
                Estado                  = p.Estado,
                EquipoLocalNombre       = p.EquipoLocal.Nombre,
                EquipoLocalLogo         = p.EquipoLocal.Logo,
                EquipoLocalAbreviacion  = p.EquipoLocal.Abreviacion,
                EquipoVisitanteNombre   = p.EquipoVisitante.Nombre,
                EquipoVisitanteLogo     = p.EquipoVisitante.Logo,
                EquipoVisitanteAbreviacion = p.EquipoVisitante.Abreviacion,
                GolesLocal              = p.GolesLocal,
                GolesVisitante          = p.GolesVisitante
            }).ToList();

            // Clasificación: calcular de partidos finalizados
            var clasificacion = new List<ClasificacionPublicaViewModel>();
            var formatoConPuntos = torneo.Formato is "TodosContraTodos" or "GruposYEliminacion" or "FormatoChampion";
            if (formatoConPuntos)
            {
                var ptsVictoria = torneo.ConfiguracionTorneo?.PuntosPorVictoria ?? 3;
                var ptsEmpate   = torneo.ConfiguracionTorneo?.PuntosPorEmpate   ?? 1;

                // Build dict keyed by equipoId
                var stats = new Dictionary<int, ClasificacionPublicaViewModel>();

                // seed all inscribed teams
                foreach (var et in torneo.EquiposTorneos)
                {
                    var eq = et.Equipo;
                    stats[eq.Id] = new ClasificacionPublicaViewModel
                    {
                        EquipoId         = eq.Id,
                        EquipoNombre     = eq.Nombre,
                        EquipoAbreviacion = eq.Abreviacion,
                        EquipoLogo       = eq.Logo
                    };
                }

                foreach (var p in todosPartidos.Where(p => p.Estado == "Finalizado" && p.GolesLocal.HasValue && p.GolesVisitante.HasValue))
                {
                    if (!stats.ContainsKey(p.EquipoLocalId) || !stats.ContainsKey(p.EquipoVisitanteId))
                        continue;

                    var loc = stats[p.EquipoLocalId];
                    var vis = stats[p.EquipoVisitanteId];
                    int gl = p.GolesLocal!.Value, gv = p.GolesVisitante!.Value;

                    loc.PJ++; loc.GF += gl; loc.GC += gv;
                    vis.PJ++; vis.GF += gv; vis.GC += gl;

                    if (gl > gv)      { loc.G++; vis.P++; loc.Pts += ptsVictoria; }
                    else if (gl < gv) { vis.G++; loc.P++; vis.Pts += ptsVictoria; }
                    else              { loc.E++; vis.E++; loc.Pts += ptsEmpate; vis.Pts += ptsEmpate; }
                }

                clasificacion = stats.Values
                    .OrderByDescending(s => s.Pts)
                    .ThenByDescending(s => s.GD)
                    .ThenByDescending(s => s.GF)
                    .ThenBy(s => s.EquipoNombre)
                    .Select((s, i) => { s.Posicion = i + 1; return s; })
                    .ToList();
            }

            Torneo = new TorneoDetallePublicoViewModel
            {
                Id                  = torneo.Id,
                Nombre              = torneo.Nombre,
                FechaInicio         = torneo.FechaInicio,
                FechaFin            = torneo.FechaFin,
                Estado              = torneo.Estado,
                LigaNombre          = torneo.Liga.Nombre,
                CategoriaNombre     = torneo.Categoria.Nombre,
                Formato             = torneo.Formato,
                EquiposInscritos    = torneo.EquiposTorneos.Count,
                EquiposMaximo       = torneo.NumeroEquiposMaximo,
                LogoUrl             = torneo.LogoUrl,
                ImagenUrl           = torneo.ImagenUrl,
                DescripcionCompleta = torneo.DescripcionCompleta,
                Ubicacion           = torneo.Ubicacion,
                DireccionSede       = torneo.DireccionSede,
                UrlGrupoWhatsApp    = torneo.UrlGrupoWhatsApp,
                UrlReglamento       = torneo.UrlReglamento,
                ImagenesGaleria     = galeria,
                Patrocinadores      = patrocinadores,
                Frase1              = torneo.Frase1,
                Frase2              = torneo.Frase2,
                Frase3              = torneo.Frase3,
                Equipos             = equipos,
                Partidos            = partidosVM,
                Clasificacion       = clasificacion
            };

            return Page();
        }
    }
}
