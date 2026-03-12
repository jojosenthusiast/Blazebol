using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Entrenador.Partidos.Convocatoria
{
    public class VerConvocatoriaModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConvocatoriaService _convocatoriaService;

        public VerConvocatoriaModel(ApplicationDbContext context, IConvocatoriaService convocatoriaService)
        {
            _context = context;
            _convocatoriaService = convocatoriaService;
        }

        [BindProperty(SupportsGet = true)]
        public int ConvocatoriaId { get; set; }

        public Data.Entities.Convocatoria? Convocatoria { get; set; }
        public List<ConvocatoriaJugador> Titulares { get; set; } = new();
        public List<ConvocatoriaJugador> Suplentes { get; set; } = new();
        public List<ConvocatoriaJugador> Reservas { get; set; } = new();
        public string NombrePartido { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            Convocatoria = await _context.Convocatorias
                .Include(c => c.Partido).ThenInclude(p => p.EquipoLocal)
                .Include(c => c.Partido).ThenInclude(p => p.EquipoVisitante)
                .Include(c => c.Partido).ThenInclude(p => p.Torneo)
                .Include(c => c.Equipo)
                .Include(c => c.Entrenador).ThenInclude(e => e.Persona)
                .Include(c => c.ConvocatoriaJugadores)
                    .ThenInclude(cj => cj.EquipoJugadorTorneo)
                        .ThenInclude(ejt => ejt.Jugador)
                            .ThenInclude(j => j.Persona)
                .FirstOrDefaultAsync(c => c.Id == ConvocatoriaId);

            if (Convocatoria == null) return NotFound();

            NombrePartido = $"{Convocatoria.Partido.EquipoLocal.Nombre} vs {Convocatoria.Partido.EquipoVisitante.Nombre}";

            var jugadores = Convocatoria.ConvocatoriaJugadores
                .OrderBy(cj => cj.EquipoJugadorTorneo.NumeroCamiseta)
                .ToList();

            Titulares = jugadores.Where(cj => cj.Rol == RolesConvocatoria.Titular).ToList();
            Suplentes = jugadores.Where(cj => cj.Rol == RolesConvocatoria.Suplente).ToList();
            Reservas = jugadores.Where(cj => cj.Rol == RolesConvocatoria.Reserva).ToList();

            return Page();
        }
    }
}
