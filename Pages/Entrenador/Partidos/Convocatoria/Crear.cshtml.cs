using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Entrenador.Partidos.Convocatoria
{
    public class JugadorConvocatoriaVM
    {
        public int EjtId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int NumeroCamiseta { get; set; }
        public string PosicionPreferida { get; set; } = string.Empty;
        public bool Seleccionado { get; set; }
        public string Rol { get; set; } = "Titular";
        public string PosicionAsignada { get; set; } = string.Empty;
    }

    public class PartidoInfoVM
    {
        public int Id { get; set; }
        public string NombreLocal { get; set; } = string.Empty;
        public string? LogoLocal { get; set; }
        public string? ColorLocal { get; set; }
        public string NombreVisitante { get; set; } = string.Empty;
        public string? LogoVisitante { get; set; }
        public string? ColorVisitante { get; set; }
        public DateTime Fecha { get; set; }
        public string? Sede { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
    }

    public class CrearConvocatoriaModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConvocatoriaService _convocatoriaService;
        private readonly INotificacionService _notificacionService;

        public CrearConvocatoriaModel(
            ApplicationDbContext context,
            IConvocatoriaService convocatoriaService,
            INotificacionService notificacionService)
        {
            _context = context;
            _convocatoriaService = convocatoriaService;
            _notificacionService = notificacionService;
        }

        [BindProperty(SupportsGet = true)]
        public int PartidoId { get; set; }

        public PartidoInfoVM? Partido { get; set; }
        public List<JugadorConvocatoriaVM> Jugadores { get; set; } = new();
        public int TitularesMinimo { get; set; } = 11;
        public int MaxConvocados { get; set; } = 18;
        public int ConvocatoriaId { get; set; }
        public string? Observaciones { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        private int? _entrenadorId;
        private int? _equipoId;

        private async Task<(int? entrenadorId, int? equipoId)> ObtenerEntrenadorEquipoAsync()
        {
            if (_entrenadorId.HasValue) return (_entrenadorId, _equipoId);
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return (null, null);

            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                    .ThenInclude(e => e!.EquiposEntrenadores.Where(ee => ee.Activo && ee.EsPropietario))
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            _entrenadorId = persona?.Entrenador?.Id;
            _equipoId = persona?.Entrenador?.EquiposEntrenadores.FirstOrDefault()?.EquipoId;
            return (_entrenadorId, _equipoId);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var (entrenadorId, equipoId) = await ObtenerEntrenadorEquipoAsync();
            if (!entrenadorId.HasValue || !equipoId.HasValue)
                return RedirectToPage("/Entrenador/Dashboard");

            return await CargarDatosAsync(entrenadorId.Value, equipoId.Value);
        }

        private async Task<IActionResult> CargarDatosAsync(int entrenadorId, int equipoId)
        {
            var partido = await _context.Partidos
                .Include(p => p.Torneo).ThenInclude(t => t.ConfiguracionTorneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == PartidoId);

            if (partido == null) return NotFound();

            // Validate coach owns a team in this partido
            if (partido.EquipoLocalId != equipoId && partido.EquipoVisitanteId != equipoId)
            {
                ErrorMessage = "No tienes acceso a este partido.";
                return Page();
            }

            Partido = new PartidoInfoVM
            {
                Id = partido.Id,
                NombreLocal = partido.EquipoLocal.Nombre,
                LogoLocal = partido.EquipoLocal.Logo,
                ColorLocal = partido.EquipoLocal.ColorPrimario,
                NombreVisitante = partido.EquipoVisitante.Nombre,
                LogoVisitante = partido.EquipoVisitante.Logo,
                ColorVisitante = partido.EquipoVisitante.ColorPrimario,
                Fecha = partido.Fecha,
                Sede = partido.Sede,
                NombreTorneo = partido.Torneo.Nombre
            };

            TitularesMinimo = partido.Torneo.ConfiguracionTorneo?.MinimoJugadoresTitulares ?? 11;
            MaxConvocados = partido.Torneo.ConfiguracionTorneo?.MaximoJugadoresConvocados ?? 18;

            // Load existing convocatoria if any
            var existing = await _convocatoriaService.GetByPartidoEquipoAsync(PartidoId, equipoId);
            if (existing != null)
            {
                ConvocatoriaId = existing.Id;
                Observaciones = existing.Observaciones;
            }

            // Load all active players for the team in this torneo
            var ejts = await _context.EquiposJugadoresTorneos
                .Include(ejt => ejt.Jugador).ThenInclude(j => j.Persona)
                .Where(ejt => ejt.EquipoId == equipoId && ejt.TorneoId == partido.TorneoId && ejt.Activo)
                .OrderBy(ejt => ejt.NumeroCamiseta)
                .ToListAsync();

            var convocadosExistentes = existing?.ConvocatoriaJugadores?.ToDictionary(c => c.EquipoJugadorTorneoId)
                ?? new Dictionary<int, ConvocatoriaJugador>();

            Jugadores = ejts.Select(ejt =>
            {
                convocadosExistentes.TryGetValue(ejt.Id, out var cj);
                return new JugadorConvocatoriaVM
                {
                    EjtId = ejt.Id,
                    NombreCompleto = $"{ejt.Jugador.Persona.Nombre} {ejt.Jugador.Persona.ApellidoPaterno}",
                    NumeroCamiseta = ejt.NumeroCamiseta,
                    PosicionPreferida = ejt.Jugador.PosicionPreferida ?? "—",
                    Seleccionado = cj != null,
                    Rol = cj?.Rol ?? "Titular",
                    PosicionAsignada = cj?.PosicionAsignada ?? ejt.Jugador.PosicionPreferida ?? ""
                };
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync(
            List<int> ejtIds,
            List<string> roles,
            List<string> posiciones,
            string? observaciones)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var (entrenadorId, equipoId) = await ObtenerEntrenadorEquipoAsync();
            if (!entrenadorId.HasValue || !equipoId.HasValue) return RedirectToPage("/Account/Login");

            await GuardarConvocatoriaAsync(entrenadorId.Value, equipoId.Value, ejtIds, roles, posiciones, observaciones ?? "");
            SuccessMessage = "Convocatoria guardada como borrador.";

            return await CargarDatosAsync(entrenadorId.Value, equipoId.Value);
        }

        public async Task<IActionResult> OnPostPublicarAsync(
            List<int> ejtIds,
            List<string> roles,
            List<string> posiciones,
            string? observaciones)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var (entrenadorId, equipoId) = await ObtenerEntrenadorEquipoAsync();
            if (!entrenadorId.HasValue || !equipoId.HasValue) return RedirectToPage("/Account/Login");

            // Count titulares
            int titulares = roles.Count(r => r == "Titular");

            // Get torneo config for min titulares
            var partido = await _context.Partidos
                .Include(p => p.Torneo).ThenInclude(t => t.ConfiguracionTorneo)
                .FirstOrDefaultAsync(p => p.Id == PartidoId);
            int minimoTitulares = partido?.Torneo?.ConfiguracionTorneo?.MinimoJugadoresTitulares ?? 11;

            if (titulares < minimoTitulares)
            {
                var (entId, eqId) = await ObtenerEntrenadorEquipoAsync();
                ErrorMessage = $"Debes seleccionar al menos {minimoTitulares} titulares. Actualmente tienes {titulares}.";
                return await CargarDatosAsync(entId!.Value, eqId!.Value);
            }

            var convocatoriaId = await GuardarConvocatoriaAsync(entrenadorId.Value, equipoId.Value, ejtIds, roles, posiciones, observaciones ?? "");
            await _convocatoriaService.PublicarAsync(convocatoriaId, _notificacionService);

            return RedirectToPage("/Entrenador/Partidos/Index",
                new { message = "Convocatoria publicada y jugadores notificados." });
        }

        private async Task<int> GuardarConvocatoriaAsync(int entrenadorId, int equipoId, List<int> ejtIds, List<string> roles, List<string> posiciones, string observaciones)
        {
            var convocatoria = await _convocatoriaService.CrearOrActualizarAsync(PartidoId, equipoId, entrenadorId, observaciones);

            // Remove jugadores no longer selected
            var existingJugadores = await _convocatoriaService.GetJugadoresAsync(convocatoria.Id);
            foreach (var cj in existingJugadores)
            {
                if (!ejtIds.Contains(cj.EquipoJugadorTorneoId))
                    await _convocatoriaService.QuitarJugadorAsync(convocatoria.Id, cj.EquipoJugadorTorneoId);
            }

            // Add/update selected jugadores
            for (int i = 0; i < ejtIds.Count; i++)
            {
                string rol = i < roles.Count ? roles[i] : "Titular";
                string posicion = i < posiciones.Count ? posiciones[i] : "";
                await _convocatoriaService.AgregarJugadorAsync(convocatoria.Id, ejtIds[i], rol, posicion);
            }

            return convocatoria.Id;
        }
    }
}
