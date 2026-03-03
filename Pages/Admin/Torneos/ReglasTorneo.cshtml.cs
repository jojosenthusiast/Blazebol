using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin.Torneos
{
    public class ReglasTorneoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReglasTorneoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TorneoId { get; set; }
        public string NombreTorneo { get; set; } = string.Empty;
        public List<ReglaPersonalizadaTorneo> Reglas { get; set; } = new();
        public HashSet<string> NombresExistentes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public ConfiguracionTorneo? Configuracion { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        // Plantillas predefinidas que el sistema puede verificar automáticamente
        public static readonly List<PlantillaReglaViewModel> Plantillas = new()
        {
            new("Suspensión por acumulación de amarillas",
                "El jugador que acumule el número configurado de tarjetas amarillas queda suspendido un partido.",
                TiposRegla.Suspension, true),
            new("Suspensión por tarjeta roja directa",
                "El jugador que reciba tarjeta roja directa queda suspendido el número de partidos configurado.",
                TiposRegla.Suspension, true),
            new("Verificación de documentos DUI/Pasaporte",
                "El árbitro debe verificar la identidad de los jugadores antes del inicio del partido.",
                TiposRegla.Validacion, true),
            new("Mínimo de jugadores para iniciar partido",
                "El equipo debe presentar el mínimo de jugadores titulares configurado. Si no cumple, pierde por Walk-over.",
                TiposRegla.Validacion, true),
            new("Límite de jugadores extranjeros",
                "Cada equipo puede alinear un máximo de 3 jugadores extranjeros por partido.",
                TiposRegla.Restriccion, false),
            new("Transferencias entre equipos del torneo",
                "No se permiten traspasos de jugadores entre equipos inscritos en el mismo torneo.",
                TiposRegla.Restriccion, false),
            new("Expulsión por agresión directa",
                "Jugador expulsado por agresión física queda suspendido un mínimo de 3 partidos.",
                TiposRegla.Suspension, false),
            new("Protesta ante el comité",
                "Los reclamos a decisiones arbitrales deben presentarse por escrito en las 24 horas siguientes al partido.",
                TiposRegla.Custom, false),
        };

        [BindProperty]
        public CrearReglaInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int torneoId, string? message)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!string.IsNullOrEmpty(message))
                SuccessMessage = message;

            return await CargarPaginaAsync(torneoId);
        }

        public async Task<IActionResult> OnPostCrearAsync(int torneoId)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid)
                return await CargarPaginaAsync(torneoId);

            var torneo = await _context.Torneos.FindAsync(torneoId);
            if (torneo == null) return NotFound();

            // Prevenir duplicados por nombre
            var nombreTrim = Input.Nombre.Trim();
            var duplicado = await _context.ReglasPersonalizadasTorneo
                .AnyAsync(r => r.TorneoId == torneoId
                            && r.Nombre.ToLower() == nombreTrim.ToLower());
            if (duplicado)
            {
                ModelState.AddModelError("", $"Ya existe una regla con el nombre \"{nombreTrim}\" en este torneo.");
                return await CargarPaginaAsync(torneoId);
            }

            var ultimoOrden = await _context.ReglasPersonalizadasTorneo
                .Where(r => r.TorneoId == torneoId)
                .MaxAsync(r => (int?)r.Orden) ?? 0;

            _context.ReglasPersonalizadasTorneo.Add(new ReglaPersonalizadaTorneo
            {
                TorneoId = torneoId,
                Nombre = nombreTrim,
                Descripcion = Input.Descripcion.Trim(),
                Tipo = Input.Tipo,
                EsAutomatica = Input.EsAutomatica,
                Activa = true,
                Orden = ultimoOrden + 1,
                FechaCreacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return RedirectToPage(new { torneoId, message = "Regla creada exitosamente." });
        }

        public async Task<IActionResult> OnPostAgregarPlantillaAsync(int torneoId, string nombre)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var plantilla = Plantillas.FirstOrDefault(p =>
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            if (plantilla == null) return BadRequest();

            // Verificar duplicado
            var existe = await _context.ReglasPersonalizadasTorneo
                .AnyAsync(r => r.TorneoId == torneoId
                            && r.Nombre.ToLower() == plantilla.Nombre.ToLower());
            if (existe)
                return RedirectToPage(new { torneoId, message = $"La regla \"{plantilla.Nombre}\" ya está agregada." });

            var ultimoOrden = await _context.ReglasPersonalizadasTorneo
                .Where(r => r.TorneoId == torneoId)
                .MaxAsync(r => (int?)r.Orden) ?? 0;

            _context.ReglasPersonalizadasTorneo.Add(new ReglaPersonalizadaTorneo
            {
                TorneoId = torneoId,
                Nombre = plantilla.Nombre,
                Descripcion = plantilla.Descripcion,
                Tipo = plantilla.Tipo,
                EsAutomatica = plantilla.EsAutomatica,
                Activa = true,
                Orden = ultimoOrden + 1,
                FechaCreacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return RedirectToPage(new { torneoId, message = $"Regla \"{plantilla.Nombre}\" agregada." });
        }

        public async Task<IActionResult> OnPostEliminarAsync(int torneoId, int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var regla = await _context.ReglasPersonalizadasTorneo
                .FirstOrDefaultAsync(r => r.Id == id && r.TorneoId == torneoId);

            if (regla != null)
            {
                _context.ReglasPersonalizadasTorneo.Remove(regla);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { torneoId, message = "Regla eliminada." });
        }

        public async Task<IActionResult> OnPostToggleActivaAsync(int torneoId, int id)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
                return RedirectToPage("/Account/Login");

            var regla = await _context.ReglasPersonalizadasTorneo
                .FirstOrDefaultAsync(r => r.Id == id && r.TorneoId == torneoId);

            if (regla != null)
            {
                regla.Activa = !regla.Activa;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { torneoId });
        }

        private async Task<IActionResult> CargarPaginaAsync(int torneoId)
        {
            var torneo = await _context.Torneos
                .Include(t => t.ConfiguracionTorneo)
                .FirstOrDefaultAsync(t => t.Id == torneoId);

            if (torneo == null) return NotFound();

            TorneoId = torneoId;
            NombreTorneo = torneo.Nombre;
            Configuracion = torneo.ConfiguracionTorneo;

            Reglas = await _context.ReglasPersonalizadasTorneo
                .Where(r => r.TorneoId == torneoId)
                .OrderBy(r => r.Orden)
                .ToListAsync();

            NombresExistentes = new HashSet<string>(
                Reglas.Select(r => r.Nombre),
                StringComparer.OrdinalIgnoreCase);

            return Page();
        }
    }

    public class PlantillaReglaViewModel
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public bool EsAutomatica { get; set; }

        public PlantillaReglaViewModel(string nombre, string descripcion, string tipo, bool esAutomatica)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Tipo = tipo;
            EsAutomatica = esAutomatica;
        }
    }

    public class CrearReglaInput
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(500, ErrorMessage = "Máximo 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione un tipo")]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = TiposRegla.Custom;

        [Display(Name = "Validación Automática")]
        public bool EsAutomatica { get; set; } = false;
    }
}
