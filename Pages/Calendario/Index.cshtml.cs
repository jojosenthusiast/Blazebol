using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using System.Text.Json;

namespace ControlTorneosFutbol.Pages.Calendario
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)] public int Mes { get; set; }
        [BindProperty(SupportsGet = true)] public int Anio { get; set; }
        [BindProperty(SupportsGet = true)] public int? TorneoFiltroId { get; set; }

        public string UserRole { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime MesActual { get; private set; }
        public DateTime MesSiguiente { get; private set; }

        public List<TorneoFiltroVM> TorneosDisponibles { get; set; } = new();
        public List<PartidoCalendarioVM> Partidos { get; set; } = new();
        public List<PartidoCalendarioVM> PartidosHoy { get; set; } = new();
        public List<PartidoCalendarioVM> ProximosSietePartidos { get; set; } = new();
        public PartidoCalendarioVM? ProximoMiPartido { get; set; }
        public int TotalPartidosMes { get; set; }
        public int TotalMisPartidosMes { get; set; }
        public string PartidosJson { get; set; } = "[]";

        // Paleta de colores asignada por TorneoId
        private static readonly string[] TorneoColors =
        {
            "#a3ff12", "#5ac8fa", "#ff9500", "#bf5af2", "#ff2d55", "#34c759", "#ffd60a"
        };
        public static string GetTorneoColor(int torneoId) =>
            TorneoColors[Math.Abs(torneoId) % TorneoColors.Length];

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return RedirectToPage("/Account/Login");

            UserRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            NombreUsuario = HttpContext.Session.GetUserName() ?? string.Empty;

            var hoy = DateTime.UtcNow.Date;

            // Inicializar o clampar mes/año
            if (Mes < 1 || Mes > 12 || Anio < 2024 || Anio > 2035)
            {
                Mes = hoy.Month;
                Anio = hoy.Year;
            }

            MesActual = new DateTime(Anio, Mes, 1);
            MesSiguiente = MesActual.AddMonths(1);
            var fechaFin = MesActual.AddMonths(2); // rango: 2 meses

            // Torneos disponibles para filtro (activos primero)
            TorneosDisponibles = await _context.Torneos
                .OrderByDescending(t => t.Estado == EstadosTorneo.Activo)
                .ThenByDescending(t => t.FechaInicio)
                .Select(t => new TorneoFiltroVM { Id = t.Id, Nombre = t.Nombre, Estado = t.Estado })
                .ToListAsync();

            // Construir query de partidos en el rango de 2 meses
            var query = _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Where(p => p.Fecha >= MesActual && p.Fecha < fechaFin);

            if (TorneoFiltroId.HasValue)
                query = query.Where(p => p.TorneoId == TorneoFiltroId.Value);

            var partidosRaw = await query.OrderBy(p => p.Fecha).ToListAsync();

            // Determinar "mis partidos" según rol
            int? miEquipoId = null;
            int? miArbitroId = null;
            var userId = HttpContext.Session.GetUserId();

            if (userId.HasValue)
            {
                var persona = await _context.Personas
                    .Include(p => p.Jugador)
                    .Include(p => p.Entrenador)
                    .Include(p => p.Arbitro)
                    .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

                if (persona != null)
                {
                    if (UserRole == "Jugador" && persona.Jugador != null)
                    {
                        var ejt = await _context.EquiposJugadoresTorneos
                            .Where(e => e.JugadorId == persona.Jugador.Id && e.Activo)
                            .OrderByDescending(e => e.FechaInscripcion)
                            .FirstOrDefaultAsync();
                        miEquipoId = ejt?.EquipoId;
                    }
                    else if (UserRole == "Entrenador" && persona.Entrenador != null)
                    {
                        var ee = await _context.EquiposEntrenadores
                            .Where(e => e.EntrenadorId == persona.Entrenador.Id && e.Activo)
                            .FirstOrDefaultAsync();
                        miEquipoId = ee?.EquipoId;
                    }
                    else if (UserRole == "Arbitro" && persona.Arbitro != null)
                    {
                        miArbitroId = persona.Arbitro.Id;
                    }
                }
            }

            // Mapear partidos a ViewModel
            Partidos = partidosRaw.Select(p => new PartidoCalendarioVM
            {
                Id = p.Id,
                NombreLocal = p.EquipoLocal.Nombre,
                NombreVisitante = p.EquipoVisitante.Nombre,
                AbrevLocal = p.EquipoLocal.Abreviacion
                    ?? p.EquipoLocal.Nombre.Substring(0, Math.Min(3, p.EquipoLocal.Nombre.Length)).ToUpper(),
                AbrevVisitante = p.EquipoVisitante.Abreviacion
                    ?? p.EquipoVisitante.Nombre.Substring(0, Math.Min(3, p.EquipoVisitante.Nombre.Length)).ToUpper(),
                NombreTorneo = p.Torneo.Nombre,
                TorneoId = p.TorneoId,
                Estado = p.Estado,
                Fecha = p.Fecha,
                Sede = p.Sede,
                GolesLocal = p.GolesLocal,
                GolesVisitante = p.GolesVisitante,
                ColorTorneo = GetTorneoColor(p.TorneoId),
                EsMiPartido = miEquipoId.HasValue
                    ? (p.EquipoLocalId == miEquipoId || p.EquipoVisitanteId == miEquipoId)
                    : miArbitroId.HasValue
                        ? p.ArbitroId == miArbitroId
                        : false
            }).ToList();

            PartidosHoy = Partidos.Where(p => p.Fecha.Date == hoy).ToList();

            ProximosSietePartidos = Partidos
                .Where(p => p.Fecha >= DateTime.UtcNow && p.Estado != EstadosPartido.Cancelado)
                .OrderBy(p => p.Fecha)
                .Take(7)
                .ToList();

            ProximoMiPartido = Partidos
                .Where(p => p.EsMiPartido && p.Fecha >= DateTime.UtcNow
                         && p.Estado == EstadosPartido.Programado)
                .OrderBy(p => p.Fecha)
                .FirstOrDefault();

            TotalPartidosMes = Partidos.Count(p => p.Fecha.Month == Mes && p.Fecha.Year == Anio);
            TotalMisPartidosMes = Partidos.Count(p => p.EsMiPartido && p.Fecha.Month == Mes && p.Fecha.Year == Anio);

            // Serializar para JavaScript
            PartidosJson = JsonSerializer.Serialize(Partidos.Select(p => new
            {
                id = p.Id,
                fecha = p.Fecha.ToString("yyyy-MM-dd"),
                hora = p.Fecha.ToString("HH:mm"),
                local = p.NombreLocal,
                visitante = p.NombreVisitante,
                abrevLocal = p.AbrevLocal,
                abrevVisitante = p.AbrevVisitante,
                torneo = p.NombreTorneo,
                torneoId = p.TorneoId,
                estado = p.Estado,
                sede = p.Sede ?? "",
                esMio = p.EsMiPartido,
                color = p.ColorTorneo,
                golesLocal = (object?)p.GolesLocal,
                golesVisitante = (object?)p.GolesVisitante
            }));

            return Page();
        }
    }

    public class PartidoCalendarioVM
    {
        public int Id { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string AbrevLocal { get; set; } = string.Empty;
        public string AbrevVisitante { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public int TorneoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Sede { get; set; }
        public bool EsMiPartido { get; set; }
        public string ColorTorneo { get; set; } = "#a3ff12";
        public int? GolesLocal { get; set; }
        public int? GolesVisitante { get; set; }
    }

    public class TorneoFiltroVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    // ViewModel compartido para mini-widgets en dashboards
    public class ProximoPartidoDashVM
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string AbrevLocal { get; set; } = string.Empty;
        public string AbrevVisitante { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public int TorneoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Sede { get; set; }
        public bool EsMiPartido { get; set; }
        public string ColorTorneo { get; set; } = "#a3ff12";
    }
}
