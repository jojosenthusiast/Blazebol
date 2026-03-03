using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Arbitro.Partidos
{
    public class JugadoresModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public JugadoresModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int PartidoId { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public DateTime FechaPartido { get; set; }
        public string Estado { get; set; } = string.Empty;

        public List<JugadorListaViewModel> JugadoresLocal { get; set; } = new();
        public List<JugadorListaViewModel> JugadoresVisitante { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToPage("/Account/Login");

            // Get árbitro from session
            var persona = await _context.Personas
                .Include(p => p.Arbitro)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona == null || persona.Arbitro == null)
                return RedirectToPage("/Error");

            if (persona.Arbitro.EstadoAprobacion != EstadosAprobacionArbitro.Aprobado)
                return RedirectToPage("/Arbitro/Partidos/Index");

            // Load the match
            var partido = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partido == null)
                return NotFound();

            // Verify this árbitro is assigned to the match
            if (partido.ArbitroId != persona.Arbitro.Id)
                return RedirectToPage("/Arbitro/Partidos/Index");

            PartidoId = partido.Id;
            NombreLocal = partido.EquipoLocal.Nombre;
            NombreVisitante = partido.EquipoVisitante.Nombre;
            NombreTorneo = partido.Torneo.Nombre;
            FechaPartido = partido.Fecha;
            Estado = partido.Estado;

            // Load active players for local team
            JugadoresLocal = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoLocalId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new JugadorListaViewModel
                {
                    NombreCompleto = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    Posicion = ejt.Jugador.PosicionPreferida ?? "—",
                    Estado = ejt.Activo ? "Activo" : "Baja"
                })
                .ToListAsync();

            // Load active players for visitante team
            JugadoresVisitante = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoVisitanteId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new JugadorListaViewModel
                {
                    NombreCompleto = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    Posicion = ejt.Jugador.PosicionPreferida ?? "—",
                    Estado = ejt.Activo ? "Activo" : "Baja"
                })
                .ToListAsync();

            return Page();
        }
    }

    public class JugadorListaViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public int NumeroCamiseta { get; set; }
        public string Posicion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
