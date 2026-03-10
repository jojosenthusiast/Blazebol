using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Pages.Calendario;

namespace ControlTorneosFutbol.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string UserName { get; set; } = string.Empty;

        // Métricas principales
        public int TotalTorneos { get; set; }
        public int TorneosNuevosEsteMes { get; set; }
        public int TorneosActivos { get; set; }
        public int TotalEquipos { get; set; }
        public int EquiposNuevosEstaSemana { get; set; }
        public int TotalJugadores { get; set; }
        public int TotalArbitros { get; set; }
        public int ArbitrosPendientesAprobacion { get; set; }
        public int AccesosHoy { get; set; }
        public int IntentosFallidos { get; set; }
        public int UsuariosBloqueados { get; set; }

        // Listas
        public List<TorneoItemViewModel> UltimosTorneos { get; set; } = new();
        public List<ProximoPartidoDashVM> ProximosPartidos { get; set; } = new();
        public List<ActividadRecienteVM> ActividadReciente { get; set; } = new();
        public List<TorneoActivoVM> TorneosActivosList { get; set; } = new();

        private static readonly string[] _torneoColors = { "#a3ff12", "#5ac8fa", "#ff9500", "#bf5af2", "#ff2d55", "#34c759", "#ffd60a" };
        private static string GetTorneoColor(int id) => _torneoColors[Math.Abs(id) % _torneoColors.Length];

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return RedirectToPage("/Account/Login");

            if (!HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Error", new { statusCode = 403 });

            UserName = HttpContext.Session.GetUserName() ?? "Administrador";

            var ahora       = DateTime.UtcNow;
            var hoy         = ahora.Date;
            var inicioMes   = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);

            // ── Métricas ──────────────────────────────────────────────────
            TotalTorneos             = await _context.Torneos.CountAsync();
            TorneosNuevosEsteMes     = await _context.Torneos.CountAsync(t => t.FechaCreacion >= inicioMes);
            TorneosActivos           = await _context.Torneos.CountAsync(t => t.Estado == "Activo");
            TotalEquipos             = await _context.Equipos.CountAsync(e => e.Activo);
            EquiposNuevosEstaSemana  = await _context.Equipos.CountAsync(e => e.Activo && e.FechaCreacion >= inicioSemana);
            TotalJugadores           = await _context.Jugadores.CountAsync();
            TotalArbitros            = await _context.Arbitros.CountAsync();
            ArbitrosPendientesAprobacion = await _context.Arbitros.CountAsync(a => a.EstadoAprobacion == "Pendiente");

            var auditoriaHoy = await _context.AuditoriaAccesos
                .Where(a => a.FechaIntento >= hoy)
                .ToListAsync();

            AccesosHoy       = auditoriaHoy.Count(a => a.Exitoso);
            IntentosFallidos = auditoriaHoy.Count(a => !a.Exitoso);
            UsuariosBloqueados = await _context.Usuarios.CountAsync(u =>
                u.FechaBloqueoFin.HasValue && u.FechaBloqueoFin > DateTime.UtcNow);

            // ── Últimos torneos ───────────────────────────────────────────
            UltimosTorneos = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.FechaCreacion)
                .Take(5)
                .Select(t => new TorneoItemViewModel
                {
                    Id               = t.Id,
                    Nombre           = t.Nombre,
                    NombreLiga       = t.Liga.Nombre,
                    NombreTemporada  = t.Temporada.Nombre,
                    NombreCategoria  = t.Categoria.Nombre,
                    Formato          = t.Formato,
                    Estado           = t.Estado,
                    FechaInicio      = t.FechaInicio,
                    FechaFin         = t.FechaFin,
                    NumeroEquiposMaximo = t.NumeroEquiposMaximo,
                    PuedeEditar      = t.Estado != "Finalizado"
                })
                .ToListAsync();

            // ── Próximos partidos ─────────────────────────────────────────
            ProximosPartidos = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Where(p => p.Estado == EstadosPartido.Programado && p.Fecha >= ahora)
                .OrderBy(p => p.Fecha)
                .Take(5)
                .Select(p => new ProximoPartidoDashVM
                {
                    Id               = p.Id,
                    Fecha            = p.Fecha,
                    NombreTorneo     = p.Torneo.Nombre,
                    TorneoId         = p.TorneoId,
                    NombreLocal      = p.EquipoLocal.Nombre,
                    NombreVisitante  = p.EquipoVisitante.Nombre,
                    AbrevLocal       = p.EquipoLocal.Abreviacion ?? p.EquipoLocal.Nombre.Substring(0, Math.Min(3, p.EquipoLocal.Nombre.Length)).ToUpper(),
                    AbrevVisitante   = p.EquipoVisitante.Abreviacion ?? p.EquipoVisitante.Nombre.Substring(0, Math.Min(3, p.EquipoVisitante.Nombre.Length)).ToUpper(),
                    Estado           = p.Estado,
                    Sede             = p.Sede
                })
                .ToListAsync();

            foreach (var p in ProximosPartidos)
                p.ColorTorneo = GetTorneoColor(p.TorneoId);

            // ── Actividad reciente (últimos 5 eventos de auditoría) ───────
            ActividadReciente = await _context.AuditoriaAccesos
                .OrderByDescending(a => a.FechaIntento)
                .Take(5)
                .Select(a => new ActividadRecienteVM
                {
                    Email       = a.Email ?? "—",
                    Exitoso     = a.Exitoso,
                    Fecha       = a.FechaIntento,
                    MotivoFallo = a.MotivoFallo
                })
                .ToListAsync();

            // ── Torneos activos (sidebar panel) ───────────────────────────
            TorneosActivosList = await _context.Torneos
                .Include(t => t.EquiposTorneos)
                .Where(t => t.Estado == "Activo")
                .OrderByDescending(t => t.FechaInicio)
                .Take(4)
                .Select(t => new TorneoActivoVM
                {
                    Id         = t.Id,
                    Nombre     = t.Nombre,
                    NumEquipos = t.EquiposTorneos.Count,
                    FechaFin   = t.FechaFin
                })
                .ToListAsync();

            return Page();
        }
    }

    public class ActividadRecienteVM
    {
        public string   Email       { get; set; } = string.Empty;
        public bool     Exitoso     { get; set; }
        public DateTime Fecha       { get; set; }
        public string?  MotivoFallo { get; set; }

        public string IconClass   => Exitoso ? "bi-box-arrow-in-right" : "bi-x-circle";
        public string IconColor   => Exitoso ? "var(--color-success)" : "var(--color-error)";

        public string TiempoRelativo
        {
            get
            {
                var diff = DateTime.UtcNow - Fecha;
                if (diff.TotalMinutes < 1)  return "Ahora mismo";
                if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
                if (diff.TotalHours   < 24) return $"Hace {(int)diff.TotalHours} h";
                return Fecha.ToString("dd/MM HH:mm");
            }
        }
    }

    public class TorneoActivoVM
    {
        public int      Id         { get; set; }
        public string   Nombre     { get; set; } = string.Empty;
        public int      NumEquipos { get; set; }
        public DateTime FechaFin   { get; set; }
    }
}
