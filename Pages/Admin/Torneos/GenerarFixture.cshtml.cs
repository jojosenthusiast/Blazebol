using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class GenerarFixtureModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IFixtureService _fixture;

        public GenerarFixtureModel(ApplicationDbContext db, IFixtureService fixture)
        {
            _db = db;
            _fixture = fixture;
        }

        public Torneo Torneo { get; set; } = null!;
        public ConfiguracionTorneo Config { get; set; } = null!;
        public List<Equipo> Equipos { get; set; } = new();
        public bool TieneFixture { get; set; }
        public int TotalPartidos { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!await CargarDatosAsync(id))
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!await CargarDatosAsync(id))
                return NotFound();

            // Validaciones previas a la generación
            if (Equipos.Count < 2)
            {
                ErrorMessage = "Se necesitan al menos 2 equipos inscritos para generar el fixture.";
                return Page();
            }

            if (TieneFixture)
            {
                // Verificar si hay partidos ya jugados — no permitir regenerar
                var jugados = await _db.Partidos
                    .CountAsync(p => p.TorneoId == id && p.Estado == EstadosPartido.Finalizado);
                if (jugados > 0)
                {
                    ErrorMessage = $"No se puede regenerar el fixture porque hay {jugados} partido(s) ya finalizados.";
                    return Page();
                }
            }

            try
            {
                List<Partido> partidos;
                var equipoIds = Equipos.Select(e => e.Id).ToList();

                partidos = Torneo.Formato switch
                {
                    FormatosTorneo.TodosContraTodos => await _fixture.GenerarTodosContraTodos(id, Config, equipoIds),
                    FormatosTorneo.EliminacionDirecta => await _fixture.GenerarEliminacionDirecta(id, equipoIds),
                    FormatosTorneo.GruposYEliminacion => await _fixture.GenerarGrupos(id, Config, equipoIds),
                    _ => throw new InvalidOperationException($"Formato '{Torneo.Formato}' no soportado para fixture automático.")
                };

                int total = await _fixture.GuardarFixture(id, partidos);
                return RedirectToPage("/Admin/Torneos/AsignarDetallesFixture",
                    new { id, message = $"Fixture generado: {total} partido(s) creados." });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al generar fixture: {ex.Message}";
                return Page();
            }
        }

        private async Task<bool> CargarDatosAsync(int torneoId)
        {
            var torneo = await _db.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Categoria)
                .Include(t => t.ConfiguracionTorneo)
                .FirstOrDefaultAsync(t => t.Id == torneoId);

            if (torneo == null)
                return false;

            Torneo = torneo;
            Config = torneo.ConfiguracionTorneo ?? new ConfiguracionTorneo();

            Equipos = await _db.EquiposTorneos
                .Where(et => et.TorneoId == torneoId && et.Estado == "Activo")
                .Include(et => et.Equipo)
                .Select(et => et.Equipo)
                .ToListAsync();

            TieneFixture = await _fixture.TieneFixture(torneoId);
            if (TieneFixture)
            {
                var (total, _, _) = await _fixture.EstadisticasFixture(torneoId);
                TotalPartidos = total;
            }

            return true;
        }
    }
}
