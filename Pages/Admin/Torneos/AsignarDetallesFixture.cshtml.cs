using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class AsignarDetallesFixtureModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IFixtureService _fixture;

        public AsignarDetallesFixtureModel(ApplicationDbContext db, IFixtureService fixture)
        {
            _db = db;
            _fixture = fixture;
        }

        public Torneo Torneo { get; set; } = null!;
        public List<PartidoDetalleRow> Partidos { get; set; } = new();
        public List<ControlTorneosFutbol.Data.Entities.Arbitro> ArbitrosDisponibles { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public class PartidoDetalleRow
        {
            public int Id { get; set; }
            public string? Fase { get; set; }
            public int? Jornada { get; set; }
            public string EquipoLocal { get; set; } = string.Empty;
            public string EquipoVisitante { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public string? Sede { get; set; }
            public int? ArbitroId { get; set; }
            public string Estado { get; set; } = string.Empty;
        }

        // Input model for batch update
        public class DetalleInput
        {
            public int PartidoId { get; set; }
            public DateTime Fecha { get; set; }
            public string? Sede { get; set; }
            public int? ArbitroId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            if (!await CargarDatosAsync(id))
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, [FromForm] List<DetalleInput> detalles)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!await CargarDatosAsync(id))
                return NotFound();

            if (detalles == null || !detalles.Any())
            {
                SuccessMessage = "No se realizaron cambios.";
                return Page();
            }

            var ids = detalles.Select(d => d.PartidoId).ToList();
            var partidos = await _db.Partidos
                .Where(p => p.TorneoId == id && ids.Contains(p.Id))
                .ToListAsync();

            foreach (var d in detalles)
            {
                var partido = partidos.FirstOrDefault(p => p.Id == d.PartidoId);
                if (partido == null || partido.Estado == EstadosPartido.Finalizado)
                    continue;

                partido.Fecha = d.Fecha;
                partido.Sede = d.Sede;
                partido.ArbitroId = d.ArbitroId == 0 ? null : d.ArbitroId;
                partido.FechaActualizacion = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // Recargar para mostrar datos actualizados
            await CargarDatosAsync(id);
            SuccessMessage = "Detalles guardados exitosamente.";
            return Page();
        }

        private async Task<bool> CargarDatosAsync(int torneoId)
        {
            var torneo = await _db.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Categoria)
                .FirstOrDefaultAsync(t => t.Id == torneoId);

            if (torneo == null)
                return false;

            Torneo = torneo;

            Partidos = await _db.Partidos
                .Where(p => p.TorneoId == torneoId)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .OrderBy(p => p.Fase)
                .ThenBy(p => p.Jornada)
                .ThenBy(p => p.Id)
                .Select(p => new PartidoDetalleRow
                {
                    Id             = p.Id,
                    Fase           = p.Fase,
                    Jornada        = p.Jornada,
                    EquipoLocal    = p.EquipoLocal.Nombre,
                    EquipoVisitante = p.EquipoVisitante.Nombre,
                    Fecha          = p.Fecha,
                    Sede           = p.Sede,
                    ArbitroId      = p.ArbitroId,
                    Estado         = p.Estado
                })
                .ToListAsync();

            ArbitrosDisponibles = await _db.Arbitros
                .Where(a => a.EstadoAprobacion == "Aprobado")
                .Include(a => a.Persona)
                .OrderBy(a => a.Persona.Nombre)
                .ToListAsync();

            return true;
        }
    }
}
