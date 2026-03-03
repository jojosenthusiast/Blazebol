using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Entrenador.Plantilla
{
    public class AgregarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AgregarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int EquipoId { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public List<TorneoDropdownItem> TorneosDisponibles { get; set; } = new();
        public List<JugadorResultadoViewModel> Resultados { get; set; } = new();
        public string? BuscarTerm { get; set; }
        public int? TorneoSeleccionado { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        private async Task<(Data.Entities.Entrenador? entrenador, int equipoId, string nombreEquipo)?> ObtenerContextoAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return null;

            var persona = await _context.Personas
                .Include(p => p.Entrenador)
                .FirstOrDefaultAsync(p => p.UsuarioId == userId.Value);

            if (persona?.Entrenador == null) return null;

            var ee = await _context.EquiposEntrenadores
                .Include(e => e.Equipo)
                .Where(e => e.EntrenadorId == persona.Entrenador.Id && e.Activo)
                .OrderByDescending(e => e.EsPropietario)
                .FirstOrDefaultAsync();

            if (ee == null) return null;
            return (persona.Entrenador, ee.EquipoId, ee.Equipo.Nombre);
        }

        public async Task<IActionResult> OnGetAsync(int? torneoId, string? buscar, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            var ctx = await ObtenerContextoAsync();
            if (ctx == null)
            {
                ErrorMessage = "No tienes un equipo asignado.";
                return Page();
            }

            var (_, equipoId, nombreEquipo) = ctx.Value;
            EquipoId = equipoId;
            NombreEquipo = nombreEquipo;
            BuscarTerm = buscar;
            TorneoSeleccionado = torneoId;

            TorneosDisponibles = await _context.EquiposTorneos
                .Include(et => et.Torneo)
                .Where(et => et.EquipoId == equipoId && et.Torneo.Estado != "Finalizado")
                .Select(et => new TorneoDropdownItem { TorneoId = et.TorneoId, Nombre = et.Torneo.Nombre, Estado = et.Torneo.Estado })
                .ToListAsync();

            if (torneoId.HasValue)
            {
                var jugadoresQuery = _context.Jugadores
                    .Include(j => j.Persona)
                        .ThenInclude(p => p.Usuario)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    var termLower = buscar.ToLower().Trim();
                    jugadoresQuery = jugadoresQuery.Where(j =>
                        j.Persona.Nombre.ToLower().Contains(termLower) ||
                        j.Persona.ApellidoPaterno.ToLower().Contains(termLower) ||
                        (j.Persona.ApellidoMaterno != null && j.Persona.ApellidoMaterno.ToLower().Contains(termLower)) ||
                        j.Persona.Usuario.Email.ToLower().Contains(termLower));
                }

                var jugadores = await jugadoresQuery.Take(50).ToListAsync();

                var enTorneo = await _context.EquiposJugadoresTorneos
                    .Where(ejt => ejt.TorneoId == torneoId.Value && ejt.Activo)
                    .Select(ejt => new { ejt.JugadorId, ejt.EquipoId })
                    .ToListAsync();

                var invitacionesPendientes = await _context.SolicitudesJugadorEquipo
                    .Where(s => s.EquipoId == equipoId
                             && s.TorneoId == torneoId.Value
                             && s.EsInvitacion
                             && s.Estado == EstadosSolicitud.Pendiente)
                    .Select(s => s.JugadorId)
                    .ToListAsync();

                Resultados = jugadores.Select(j =>
                {
                    var enTorneoEntry = enTorneo.FirstOrDefault(e => e.JugadorId == j.Id);
                    bool yaEnEsteEquipo = enTorneoEntry?.EquipoId == equipoId;
                    bool yaEnOtroEquipo = enTorneoEntry != null && enTorneoEntry.EquipoId != equipoId;
                    bool invitacionPendiente = invitacionesPendientes.Contains(j.Id);

                    return new JugadorResultadoViewModel
                    {
                        JugadorId = j.Id,
                        NombreCompleto = $"{j.Persona.Nombre} {j.Persona.ApellidoPaterno}",
                        Email = j.Persona.Usuario.Email,
                        PosicionPreferida = j.PosicionPreferida,
                        YaEnEsteEquipo = yaEnEsteEquipo,
                        YaEnOtroEquipo = yaEnOtroEquipo,
                        InvitacionPendiente = invitacionPendiente,
                        PuedeAgregarDirecto = !yaEnEsteEquipo && !yaEnOtroEquipo && !invitacionPendiente,
                        PuedeInvitar = yaEnOtroEquipo && !invitacionPendiente
                    };
                }).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAgregarAsync(int jugadorId, int torneoId, int numeroCamiseta)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var ctx = await ObtenerContextoAsync();
            if (ctx == null) return RedirectToPage("/Account/Login");

            var (_, equipoId, _) = ctx.Value;

            if (await _context.EquiposJugadoresTorneos.AnyAsync(ejt =>
                    ejt.JugadorId == jugadorId && ejt.TorneoId == torneoId && ejt.Activo))
            {
                ErrorMessage = "El jugador ya pertenece a un equipo en este torneo.";
                return await OnGetAsync(torneoId, null, null);
            }

            if (await _context.EquiposJugadoresTorneos.AnyAsync(ejt =>
                    ejt.EquipoId == equipoId && ejt.TorneoId == torneoId
                    && ejt.NumeroCamiseta == numeroCamiseta && ejt.Activo))
            {
                ErrorMessage = $"El número de camiseta {numeroCamiseta} ya está en uso en este torneo.";
                return await OnGetAsync(torneoId, null, null);
            }

            _context.EquiposJugadoresTorneos.Add(new EquipoJugadorTorneo
            {
                JugadorId = jugadorId,
                EquipoId = equipoId,
                TorneoId = torneoId,
                NumeroCamiseta = numeroCamiseta,
                FechaInscripcion = DateTime.UtcNow,
                Activo = true
            });
            await _context.SaveChangesAsync();

            return RedirectToPage("/Entrenador/Plantilla/Index",
                new { torneoId, message = "Jugador agregado exitosamente." });
        }

        public async Task<IActionResult> OnPostInvitarAsync(int jugadorId, int torneoId, string? mensaje)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Entrenador"))
                return RedirectToPage("/Account/Login");

            var ctx = await ObtenerContextoAsync();
            if (ctx == null) return RedirectToPage("/Account/Login");

            var (entrenador, equipoId, _) = ctx.Value;

            if (await _context.SolicitudesJugadorEquipo.AnyAsync(s =>
                    s.JugadorId == jugadorId && s.EquipoId == equipoId
                    && s.TorneoId == torneoId && s.EsInvitacion
                    && s.Estado == EstadosSolicitud.Pendiente))
            {
                ErrorMessage = "Ya existe una invitación pendiente para este jugador en este torneo.";
                return await OnGetAsync(torneoId, null, null);
            }

            _context.SolicitudesJugadorEquipo.Add(new SolicitudJugadorEquipo
            {
                JugadorId = jugadorId,
                EquipoId = equipoId,
                TorneoId = torneoId,
                EsInvitacion = true,
                MensajeJugador = mensaje,
                Estado = EstadosSolicitud.Pendiente,
                FechaSolicitud = DateTime.UtcNow,
                EntrenadorResponsableId = entrenador!.Id
            });
            await _context.SaveChangesAsync();

            return await OnGetAsync(torneoId, null, "Invitación enviada al jugador.");
        }
    }

    public class JugadorResultadoViewModel
    {
        public int JugadorId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PosicionPreferida { get; set; }
        public bool YaEnEsteEquipo { get; set; }
        public bool YaEnOtroEquipo { get; set; }
        public bool InvitacionPendiente { get; set; }
        public bool PuedeAgregarDirecto { get; set; }
        public bool PuedeInvitar { get; set; }
    }

}
