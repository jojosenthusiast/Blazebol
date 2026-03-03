using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Espectador
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string NombreUsuario { get; set; } = string.Empty;
        public int TorneosActivos { get; set; }
        public int TotalEquipos { get; set; }
        public int ProximosPartidos { get; set; }
        public List<TorneoItemViewModel> TorneosRecientes { get; set; } = new();
        public EquipoFavoritoInfoViewModel? EquipoFavorito { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Espectador"))
                return RedirectToPage("/Account/Login");

            NombreUsuario = HttpContext.Session.GetString("UserName") ?? "Espectador";

            // Stats — solo torneos Activos (excluye Borrador)
            TorneosActivos = await _context.Torneos
                .CountAsync(t => t.Estado == EstadosTorneo.Activo);

            TotalEquipos = await _context.EquiposTorneos
                .Where(et => et.Torneo.Estado == EstadosTorneo.Activo)
                .Select(et => et.EquipoId)
                .Distinct()
                .CountAsync();

            ProximosPartidos = await _context.Partidos
                .CountAsync(p => p.Estado == EstadosPartido.Programado
                              && p.Torneo.Estado == EstadosTorneo.Activo);

            // Torneos recientes — Activos y Finalizados, sin Borrador
            var torneos = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .Include(t => t.EquiposTorneos)
                .Where(t => t.Estado == EstadosTorneo.Activo || t.Estado == EstadosTorneo.Finalizado)
                .OrderByDescending(t => t.Estado == EstadosTorneo.Activo) // activos primero
                .ThenByDescending(t => t.FechaCreacion)
                .Take(6)
                .ToListAsync();

            TorneosRecientes = torneos.Select(t => new TorneoItemViewModel
            {
                Id = t.Id,
                Nombre = t.Nombre,
                NombreLiga = t.Liga.Nombre,
                NombreTemporada = t.Temporada.Nombre,
                NombreCategoria = t.Categoria.Nombre,
                Formato = t.Formato,
                Estado = t.Estado,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin,
                NumeroEquiposInscritos = t.EquiposTorneos.Count
            }).ToList();

            // Equipo favorito
            var userId = HttpContext.Session.GetUserId();
            if (userId.HasValue)
            {
                var preferencia = await _context.PreferenciasEspectador
                    .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

                if (preferencia?.EquipoFavoritoId.HasValue == true)
                    await CargarEquipoFavoritoAsync(preferencia.EquipoFavoritoId.Value);
            }

            return Page();
        }

        private async Task CargarEquipoFavoritoAsync(int equipoId)
        {
            var equipo = await _context.Equipos
                .Include(e => e.EquiposEntrenadores.Where(ee => ee.Activo && ee.EsPropietario))
                    .ThenInclude(ee => ee.Entrenador).ThenInclude(en => en.Persona)
                .FirstOrDefaultAsync(e => e.Id == equipoId);

            if (equipo == null) return;

            // Torneo activo del equipo
            var torneoActivo = await _context.EquiposTorneos
                .Include(et => et.Torneo)
                .Where(et => et.EquipoId == equipoId && et.Torneo.Estado == EstadosTorneo.Activo)
                .Select(et => et.Torneo)
                .FirstOrDefaultAsync();

            // Próximo partido
            var proximoPartido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Where(p => (p.EquipoLocalId == equipoId || p.EquipoVisitanteId == equipoId)
                         && p.Estado == EstadosPartido.Programado)
                .OrderBy(p => p.Fecha)
                .FirstOrDefaultAsync();

            // Posición en liga (si torneo activo es TodosContraTodos)
            int? posicion = null;
            int? totalEquipos = null;
            if (torneoActivo != null && torneoActivo.Formato == FormatosTorneo.TodosContraTodos)
            {
                var equiposTorneo = await _context.EquiposTorneos
                    .Where(et => et.TorneoId == torneoActivo.Id)
                    .Select(et => et.EquipoId)
                    .ToListAsync();

                totalEquipos = equiposTorneo.Count;

                var partidos = await _context.Partidos
                    .Where(p => p.TorneoId == torneoActivo.Id
                             && p.Estado == EstadosPartido.Finalizado
                             && p.GolesLocal.HasValue)
                    .ToListAsync();

                var config = await _context.ConfiguracionesTorneo
                    .FirstOrDefaultAsync(c => c.TorneoId == torneoActivo.Id);
                int ptsVic = config?.PuntosPorVictoria ?? 3;
                int ptsEmp = config?.PuntosPorEmpate ?? 1;

                var pts = equiposTorneo.ToDictionary(eId => eId, _ => 0);
                foreach (var p in partidos)
                {
                    if (!pts.ContainsKey(p.EquipoLocalId) || !pts.ContainsKey(p.EquipoVisitanteId)) continue;
                    int gl = p.GolesLocal!.Value, gv = p.GolesVisitante!.Value;
                    if (gl > gv) { pts[p.EquipoLocalId] += ptsVic; }
                    else if (gl < gv) { pts[p.EquipoVisitanteId] += ptsVic; }
                    else { pts[p.EquipoLocalId] += ptsEmp; pts[p.EquipoVisitanteId] += ptsEmp; }
                }

                var ranking = pts.OrderByDescending(kv => kv.Value).ToList();
                var idx = ranking.FindIndex(kv => kv.Key == equipoId);
                if (idx >= 0) posicion = idx + 1;
            }

            // Últimos 5 resultados
            var ultimos = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Where(p => (p.EquipoLocalId == equipoId || p.EquipoVisitanteId == equipoId)
                         && p.Estado == EstadosPartido.Finalizado
                         && p.GolesLocal.HasValue)
                .OrderByDescending(p => p.Fecha)
                .Take(5)
                .ToListAsync();

            EquipoFavorito = new EquipoFavoritoInfoViewModel
            {
                Id = equipo.Id,
                Nombre = equipo.Nombre,
                Abreviacion = equipo.Abreviacion,
                Logo = equipo.Logo,
                ColorPrimario = equipo.ColorPrimario,
                NombreEntrenador = equipo.EquiposEntrenadores.FirstOrDefault() is { } ee
                    ? $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}" : null,
                NombreTorneoActivo = torneoActivo?.Nombre,
                TorneoActivoId = torneoActivo?.Id,
                Posicion = posicion,
                TotalEquipos = totalEquipos,
                ProximoPartido = proximoPartido == null ? null : new ProximoPartidoViewModel
                {
                    Fecha = proximoPartido.Fecha,
                    NombreRival = proximoPartido.EquipoLocalId == equipoId
                        ? proximoPartido.EquipoVisitante.Nombre
                        : proximoPartido.EquipoLocal.Nombre,
                    EsLocal = proximoPartido.EquipoLocalId == equipoId,
                    Sede = proximoPartido.Sede
                },
                UltimosResultados = ultimos.Select(p => new ResultadoRecienteViewModel
                {
                    NombreRival = p.EquipoLocalId == equipoId ? p.EquipoVisitante.Nombre : p.EquipoLocal.Nombre,
                    GolesPropio = p.EquipoLocalId == equipoId ? p.GolesLocal!.Value : p.GolesVisitante!.Value,
                    GolesRival = p.EquipoLocalId == equipoId ? p.GolesVisitante!.Value : p.GolesLocal!.Value,
                    Fecha = p.Fecha
                }).ToList()
            };
        }
    }

    public class EquipoFavoritoInfoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? ColorPrimario { get; set; }
        public string? NombreEntrenador { get; set; }
        public string? NombreTorneoActivo { get; set; }
        public int? TorneoActivoId { get; set; }
        public int? Posicion { get; set; }
        public int? TotalEquipos { get; set; }
        public ProximoPartidoViewModel? ProximoPartido { get; set; }
        public List<ResultadoRecienteViewModel> UltimosResultados { get; set; } = new();
    }

    public class ProximoPartidoViewModel
    {
        public DateTime Fecha { get; set; }
        public string NombreRival { get; set; } = string.Empty;
        public bool EsLocal { get; set; }
        public string? Sede { get; set; }
    }

    public class ResultadoRecienteViewModel
    {
        public string NombreRival { get; set; } = string.Empty;
        public int GolesPropio { get; set; }
        public int GolesRival { get; set; }
        public DateTime Fecha { get; set; }
        public string Resultado => GolesPropio > GolesRival ? "G" : GolesPropio < GolesRival ? "P" : "E";
    }
}
