using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Arbitro.Partidos
{
    [IgnoreAntiforgeryToken]
    public class EnVivoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPartidoEnVivoService _enVivoService;
        private readonly INotificacionService _notificaciones;

        public EnVivoModel(ApplicationDbContext context, IPartidoEnVivoService enVivoService, INotificacionService notificaciones)
        {
            _context = context;
            _enVivoService = enVivoService;
            _notificaciones = notificaciones;
        }

        [BindProperty(SupportsGet = true)]
        public int PartidoId { get; set; }

        public PartidoEnVivoVM? Estado { get; set; }
        public int ArbitroId { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return RedirectToPage("/Account/Login");

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            var persona = await _context.Personas
                .Include(p => p.Arbitro)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Arbitro == null) return RedirectToPage("/Error");

            ArbitroId = persona.Arbitro.Id;

            var partido = await _context.Partidos.FindAsync(PartidoId);
            if (partido == null) return NotFound();

            if (partido.ArbitroId != ArbitroId)
            {
                ErrorMessage = "No estás asignado a este partido.";
                return Page();
            }

            if (partido.Estado != EstadosPartido.EnCurso)
            {
                if (partido.Estado == EstadosPartido.Finalizado)
                    return RedirectToPage("/Arbitro/Partidos/Reporte", new { partidoId = PartidoId });
                return RedirectToPage("/Arbitro/Partidos/Index");
            }

            Estado = await _enVivoService.GetEstadoEnVivoAsync(PartidoId);
            return Page();
        }

        [HttpGet]
        public async Task<IActionResult> OnGetEstadoAsync(int partidoId)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var vm = await _enVivoService.GetEstadoEnVivoAsync(partidoId);
            return new JsonResult(vm, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostGolAsync(int partidoId, int equipoId, int? ejtJugadorId, int minuto, bool esAutogol, bool esPenal)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return new JsonResult(new { ok = false });

            await _enVivoService.RegistrarIncidenciaAsync(
                partidoId, TiposIncidencia.Gol, equipoId, ejtJugadorId, minuto, null, esAutogol, esPenal);

            var vm = await _enVivoService.GetEstadoEnVivoAsync(partidoId);
            return new JsonResult(new
            {
                ok = true,
                golesLocal = vm.GolesLocal,
                golesVisitante = vm.GolesVisitante,
                incidencias = vm.Incidencias
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostTarjetaAsync(int partidoId, int equipoId, int ejtJugadorId, int minuto, string tipo)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return new JsonResult(new { ok = false });

            string tipoIncidencia = tipo == "Roja" ? TiposIncidencia.TarjetaRoja : TiposIncidencia.TarjetaAmarilla;
            await _enVivoService.RegistrarIncidenciaAsync(
                partidoId, tipoIncidencia, equipoId, ejtJugadorId, minuto, null, false, false);

            var vm = await _enVivoService.GetEstadoEnVivoAsync(partidoId);
            return new JsonResult(new { ok = true, incidencias = vm.Incidencias, jugadoresLocal = vm.JugadoresLocal, jugadoresVisitante = vm.JugadoresVisitante },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostCambioAsync(int partidoId, int equipoId, int ejtEntraId, int ejtSaleId, int minuto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return new JsonResult(new { ok = false });

            await _enVivoService.RegistrarIncidenciaAsync(
                partidoId, TiposIncidencia.Cambio, equipoId, ejtEntraId, minuto, null, false, false, ejtSaleId);

            var vm = await _enVivoService.GetEstadoEnVivoAsync(partidoId);
            return new JsonResult(new { ok = true, incidencias = vm.Incidencias, jugadoresLocal = vm.JugadoresLocal, jugadoresVisitante = vm.JugadoresVisitante },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostIncidenciaAsync(int partidoId, int equipoId, string tipo, int? ejtJugadorId, int minuto, string? desc)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return new JsonResult(new { ok = false });

            await _enVivoService.RegistrarIncidenciaAsync(
                partidoId, tipo, equipoId, ejtJugadorId, minuto, desc, false, false);

            var vm = await _enVivoService.GetEstadoEnVivoAsync(partidoId);
            return new JsonResult(new { ok = true, incidencias = vm.Incidencias },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostFinalizarAsync(int partidoId)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Arbitro"))
                return new JsonResult(new { ok = false, error = "Unauthorized" });

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return new JsonResult(new { ok = false });

            var persona = await _context.Personas
                .Include(p => p.Arbitro)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Arbitro == null) return new JsonResult(new { ok = false });

            var ok = await _enVivoService.FinalizarPartidoAsync(partidoId, persona.Arbitro.Id, _notificaciones);
            return new JsonResult(new { ok, redirectUrl = $"/Arbitro/Partidos/Reporte?partidoId={partidoId}" });
        }
    }
}
