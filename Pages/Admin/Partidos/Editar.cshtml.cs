using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Admin.Partidos
{
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificacionService _notificaciones;

        public EditarModel(ApplicationDbContext context, INotificacionService notificaciones)
        {
            _context = context;
            _notificaciones = notificaciones;
        }

        public string NombreLocal { get; set; } = string.Empty;
        public string NombreVisitante { get; set; } = string.Empty;
        public string NombreTorneo { get; set; } = string.Empty;
        public List<ArbitroDropdownItem> Arbitros { get; set; } = new();

        [BindProperty]
        public EditarPartidoInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var partido = await _context.Partidos
                .Include(p => p.Torneo)
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partido == null) return NotFound();

            await CargarArbitrosAsync();
            NombreLocal = partido.EquipoLocal.Nombre;
            NombreVisitante = partido.EquipoVisitante.Nombre;
            NombreTorneo = partido.Torneo.Nombre;

            Input = new EditarPartidoInput
            {
                Id = partido.Id,
                Estado = partido.Estado,
                GolesLocal = partido.GolesLocal,
                GolesVisitante = partido.GolesVisitante,
                Fecha = partido.Fecha,
                Sede = partido.Sede,
                ArbitroId = partido.ArbitroId,
                RazonReprogramacion = partido.RazonReprogramacion
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Torneo)
                .FirstOrDefaultAsync(p => p.Id == Input.Id);

            if (partido == null) return NotFound();

            NombreLocal = partido.EquipoLocal.Nombre;
            NombreVisitante = partido.EquipoVisitante.Nombre;
            NombreTorneo = partido.Torneo.Nombre;
            await CargarArbitrosAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Input.Estado == EstadosPartido.Finalizado &&
                (!Input.GolesLocal.HasValue || !Input.GolesVisitante.HasValue))
            {
                ModelState.AddModelError(string.Empty,
                    "Para finalizar el partido debes registrar los goles de ambos equipos.");
                return Page();
            }

            var userId = HttpContext.Session.GetUserId();

            // Detect if fecha or sede changed
            bool fechaCambio = Input.Fecha != partido.Fecha;
            bool sedeCambio = (Input.Sede ?? "") != (partido.Sede ?? "");
            bool arbitroCambio = Input.ArbitroId != partido.ArbitroId;

            // US7: conflict validation when date changes
            if (fechaCambio && Input.Estado != EstadosPartido.Cancelado)
            {
                var ventana = TimeSpan.FromHours(2);
                var conflicto = await _context.Partidos
                    .Where(p => p.Id != partido.Id
                             && p.Estado != EstadosPartido.Cancelado
                             && (
                                 // Same team involved
                                 p.EquipoLocalId == partido.EquipoLocalId ||
                                 p.EquipoVisitanteId == partido.EquipoLocalId ||
                                 p.EquipoLocalId == partido.EquipoVisitanteId ||
                                 p.EquipoVisitanteId == partido.EquipoVisitanteId ||
                                 // Same sede (non-null)
                                 (!string.IsNullOrEmpty(Input.Sede) && p.Sede == Input.Sede)
                             ))
                    .AnyAsync(p =>
                        p.Fecha >= Input.Fecha.Add(-ventana) &&
                        p.Fecha <= Input.Fecha.Add(ventana));

                if (conflicto)
                {
                    ModelState.AddModelError(string.Empty,
                        "Conflicto de horario: uno de los equipos o la sede ya tiene un partido programado en un margen de ±2 horas.");
                    return Page();
                }
            }

            // Build historial entries
            bool reprogramacion = fechaCambio &&
                (partido.Estado == EstadosPartido.Programado || partido.Estado == EstadosPartido.Reprogramado);

            string estadoAnterior = partido.Estado;

            // Apply changes
            if (fechaCambio && reprogramacion)
                partido.Estado = EstadosPartido.Reprogramado;
            else
                partido.Estado = Input.Estado;

            partido.Fecha = Input.Fecha;
            partido.Sede = string.IsNullOrWhiteSpace(Input.Sede) ? null : Input.Sede.Trim();
            partido.ArbitroId = Input.ArbitroId;
            partido.FechaActualizacion = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(Input.RazonReprogramacion))
                partido.RazonReprogramacion = Input.RazonReprogramacion.Trim();

            if (partido.Estado == EstadosPartido.Finalizado)
            {
                partido.GolesLocal = Input.GolesLocal;
                partido.GolesVisitante = Input.GolesVisitante;
            }
            else if (Input.Estado != EstadosPartido.Finalizado)
            {
                partido.GolesLocal = null;
                partido.GolesVisitante = null;
            }

            // Register HistorialPartido for each meaningful change
            if (reprogramacion)
            {
                _context.HistorialesPartido.Add(new HistorialPartido
                {
                    PartidoId = partido.Id,
                    CambiadoPorUsuarioId = userId,
                    TipoCambio = TiposCambioPartido.Reprogramacion,
                    FechaAnterior = partido.Fecha == Input.Fecha ? null : Input.Fecha, // already updated
                    FechaNueva = Input.Fecha,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = EstadosPartido.Reprogramado,
                    Razon = string.IsNullOrWhiteSpace(Input.RazonReprogramacion)
                        ? "Reprogramado por el administrador"
                        : Input.RazonReprogramacion.Trim(),
                    FechaCambio = DateTime.UtcNow
                });
            }

            if (sedeCambio)
            {
                _context.HistorialesPartido.Add(new HistorialPartido
                {
                    PartidoId = partido.Id,
                    CambiadoPorUsuarioId = userId,
                    TipoCambio = TiposCambioPartido.CambioSede,
                    SedeAnterior = partido.Sede,
                    SedeNueva = Input.Sede?.Trim(),
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = partido.Estado,
                    FechaCambio = DateTime.UtcNow
                });
            }

            if (arbitroCambio)
            {
                _context.HistorialesPartido.Add(new HistorialPartido
                {
                    PartidoId = partido.Id,
                    CambiadoPorUsuarioId = userId,
                    TipoCambio = TiposCambioPartido.CambioArbitro,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = partido.Estado,
                    FechaCambio = DateTime.UtcNow
                });
            }

            if (Input.Estado == EstadosPartido.Cancelado && estadoAnterior != EstadosPartido.Cancelado)
            {
                _context.HistorialesPartido.Add(new HistorialPartido
                {
                    PartidoId = partido.Id,
                    CambiadoPorUsuarioId = userId,
                    TipoCambio = TiposCambioPartido.Cancelacion,
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = EstadosPartido.Cancelado,
                    Razon = Input.RazonReprogramacion?.Trim(),
                    FechaCambio = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            // US7: send notifications for reprogramación or cancellation
            if (reprogramacion)
            {
                string titulo = $"Partido reprogramado: {partido.EquipoLocal.Nombre} vs {partido.EquipoVisitante.Nombre}";
                string mensaje = $"El partido fue reprogramado para el {Input.Fecha:dd/MM/yyyy HH:mm}.";
                if (!string.IsNullOrWhiteSpace(Input.RazonReprogramacion))
                    mensaje += $" Razón: {Input.RazonReprogramacion.Trim()}";
                await _notificaciones.NotificarPartidoAsync(partido.Id, titulo, mensaje, TiposNotificacion.Partido);
            }
            else if (Input.Estado == EstadosPartido.Cancelado && estadoAnterior != EstadosPartido.Cancelado)
            {
                string titulo = $"Partido cancelado: {partido.EquipoLocal.Nombre} vs {partido.EquipoVisitante.Nombre}";
                string mensaje = "El partido ha sido cancelado por el administrador.";
                if (!string.IsNullOrWhiteSpace(Input.RazonReprogramacion))
                    mensaje += $" Razón: {Input.RazonReprogramacion.Trim()}";
                await _notificaciones.NotificarPartidoAsync(partido.Id, titulo, mensaje, TiposNotificacion.Partido);
            }

            return RedirectToPage("/Admin/Partidos/Details", new { id = partido.Id });
        }

        private async Task CargarArbitrosAsync()
        {
            Arbitros = await _context.Arbitros
                .Include(a => a.Persona)
                .Where(a => a.EstadoAprobacion == EstadosAprobacionArbitro.Aprobado)
                .OrderBy(a => a.Persona.ApellidoPaterno)
                .Select(a => new ArbitroDropdownItem
                {
                    Id = a.Id,
                    NombreCompleto = $"{a.Persona.Nombre} {a.Persona.ApellidoPaterno}"
                })
                .ToListAsync();
        }
    }

    public class EditarPartidoInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = EstadosPartido.Programado;

        [Display(Name = "Goles Local")]
        [Range(0, 99, ErrorMessage = "Valor entre 0 y 99")]
        public int? GolesLocal { get; set; }

        [Display(Name = "Goles Visitante")]
        [Range(0, 99, ErrorMessage = "Valor entre 0 y 99")]
        public int? GolesVisitante { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha y Hora")]
        public DateTime Fecha { get; set; }

        [MaxLength(200)]
        [Display(Name = "Sede")]
        public string? Sede { get; set; }

        [Display(Name = "Árbitro")]
        public int? ArbitroId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Razón del cambio")]
        public string? RazonReprogramacion { get; set; }
    }
}
