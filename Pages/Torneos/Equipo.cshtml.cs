using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Torneos
{
    public class EquipoPublicoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Abreviacion { get; set; }
        public string? Logo { get; set; }
        public string? ColorPrimario { get; set; }
        public string? ColorSecundario { get; set; }
        public string? Sede { get; set; }
        public string? EntrenadorNombre { get; set; }
        public List<JugadorEquipoViewModel> Jugadores { get; set; } = new();
    }

    public class JugadorEquipoViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Posicion { get; set; }
        public int NumeroCamiseta { get; set; }
    }

    public class EquipoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EquipoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public EquipoPublicoViewModel Equipo { get; set; } = null!;
        public int TorneoId { get; set; }
        public string TorneoNombre { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
        public string DashboardPage { get; set; } = "/Account/Login";

        public async Task<IActionResult> OnGetAsync(int torneoId, int equipoId)
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

            // Verificar que el equipo esté inscrito en el torneo
            var equipoTorneo = await _context.EquiposTorneos
                .Include(et => et.Equipo)
                    .ThenInclude(e => e.EquiposEntrenadores)
                        .ThenInclude(ee => ee.Entrenador)
                            .ThenInclude(en => en.Persona)
                .Include(et => et.Torneo)
                .FirstOrDefaultAsync(et => et.TorneoId == torneoId && et.EquipoId == equipoId);

            if (equipoTorneo == null) return NotFound();

            TorneoId = torneoId;
            TorneoNombre = equipoTorneo.Torneo.Nombre;

            var e = equipoTorneo.Equipo;
            var ee = e.EquiposEntrenadores.FirstOrDefault();
            string? entrenadorNombre = null;
            if (ee != null)
                entrenadorNombre = $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}";

            // Jugadores activos inscritos en este torneo con este equipo
            var jugadoresInscritos = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador)
                    .ThenInclude(j => j.Persona)
                .Where(ejt => ejt.TorneoId == torneoId && ejt.EquipoId == equipoId && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .ToListAsync();

            Equipo = new EquipoPublicoViewModel
            {
                Id              = e.Id,
                Nombre          = e.Nombre,
                Abreviacion     = e.Abreviacion,
                Logo            = e.Logo,
                ColorPrimario   = e.ColorPrimario,
                ColorSecundario = e.ColorSecundario,
                Sede            = e.Sede,
                EntrenadorNombre = entrenadorNombre,
                Jugadores = jugadoresInscritos.Select(ejt => new JugadorEquipoViewModel
                {
                    Nombre         = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    Posicion       = ejt.Jugador.PosicionPreferida,
                    NumeroCamiseta = ejt.NumeroCamiseta
                }).ToList()
            };

            return Page();
        }
    }
}
