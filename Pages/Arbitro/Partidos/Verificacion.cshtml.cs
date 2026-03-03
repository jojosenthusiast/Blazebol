using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Arbitro.Partidos
{
    public class VerificacionModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public VerificacionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int PartidoId { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public DateTime FechaPartido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Sede { get; set; }
        public string? ErrorMessage { get; set; }

        public List<ParticipanteViewModel> JugadoresLocal { get; set; } = new();
        public List<ParticipanteViewModel> JugadoresVisitante { get; set; } = new();
        public List<ParticipanteViewModel> EntrenadoresLocal { get; set; } = new();
        public List<ParticipanteViewModel> EntrenadoresVisitante { get; set; } = new();

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
            Sede = partido.Sede;

            if (partido.Estado != EstadosPartido.Programado)
            {
                ErrorMessage = $"La verificación de identidades solo está disponible para partidos en estado \"Programado\". Este partido está en estado \"{partido.Estado}\".";
                return Page();
            }

            // Load active players for local team in this torneo
            JugadoresLocal = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoLocalId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    Rol = ejt.Jugador.PosicionPreferida ?? "—",
                    Activo = ejt.Activo
                })
                .ToListAsync();

            // Load active players for visitante team in this torneo
            JugadoresVisitante = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == partido.EquipoVisitanteId
                           && ejt.TorneoId == partido.TorneoId
                           && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .Select(ejt => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    Rol = ejt.Jugador.PosicionPreferida ?? "—",
                    Activo = ejt.Activo
                })
                .ToListAsync();

            // Load active coaches for local team
            EntrenadoresLocal = await _context.EquiposEntrenadores
                .Include(ee => ee.Entrenador).ThenInclude(e => e.Persona)
                .Where(ee => ee.EquipoId == partido.EquipoLocalId && ee.Activo)
                .OrderBy(ee => ee.Rol)
                .Select(ee => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = null,
                    Rol = ee.Rol,
                    Activo = ee.Activo
                })
                .ToListAsync();

            // Load active coaches for visitante team
            EntrenadoresVisitante = await _context.EquiposEntrenadores
                .Include(ee => ee.Entrenador).ThenInclude(e => e.Persona)
                .Where(ee => ee.EquipoId == partido.EquipoVisitanteId && ee.Activo)
                .OrderBy(ee => ee.Rol)
                .Select(ee => new ParticipanteViewModel
                {
                    NombreCompleto = $"{ee.Entrenador.Persona.Nombre} {ee.Entrenador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = null,
                    Rol = ee.Rol,
                    Activo = ee.Activo
                })
                .ToListAsync();

            return Page();
        }
    }

    public class ParticipanteViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public int? NumeroCamiseta { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
