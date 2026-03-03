using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Admin
{
    public class AuditoriaModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<AuditoriaViewModel> Registros { get; set; } = new();
        public int AccesosHoy { get; set; }
        public int FallidosHoy { get; set; }
        public int ExitososHoy { get; set; }
        public int UsuariosBloqueados { get; set; }
        public string? FiltroResultado { get; set; }
        public int FiltroDias { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(string? resultado, int dias = 1)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Administrador"))
            {
                return RedirectToPage("/Account/Login");
            }

            FiltroResultado = resultado;
            FiltroDias = dias;

            var fechaLimite = DateTime.UtcNow.AddDays(-dias);
            var hoy = DateTime.UtcNow.Date;

            // Obtener registros con filtros
            var query = _context.AuditoriaAccesos
                .Where(a => a.FechaIntento >= fechaLimite)
                .AsQueryable();

            if (!string.IsNullOrEmpty(resultado))
            {
                var exitoso = resultado == "exitoso";
                query = query.Where(a => a.Exitoso == exitoso);
            }

            Registros = await query
                .OrderByDescending(a => a.FechaIntento)
                .Select(a => new AuditoriaViewModel
                {
                    FechaIntento = a.FechaIntento,
                    Email = a.Email ?? "Desconocido",
                    DireccionIP = a.DireccionIP ?? "N/A",
                    UserAgent = a.UserAgent,
                    Exitoso = a.Exitoso,
                    MotivoFallo = a.MotivoFallo
                })
                .Take(100)
                .ToListAsync();

            // Calcular estadísticas del día
            var registrosHoy = await _context.AuditoriaAccesos
                .Where(a => a.FechaIntento >= hoy)
                .ToListAsync();

            AccesosHoy = registrosHoy.Count;
            FallidosHoy = registrosHoy.Count(a => !a.Exitoso);
            ExitososHoy = registrosHoy.Count(a => a.Exitoso);

            // Usuarios bloqueados actualmente
            UsuariosBloqueados = await _context.Usuarios
                .CountAsync(u => u.BloqueoHabilitado && 
                                u.FechaBloqueoFin.HasValue && 
                                u.FechaBloqueoFin > DateTime.UtcNow);

            return Page();
        }
    }

    // ESTA CLASE VA EN EL MISMO ARCHIVO (después de AuditoriaModel)
    public class AuditoriaViewModel
    {
        public DateTime FechaIntento { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DireccionIP { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public bool Exitoso { get; set; }
        public string? MotivoFallo { get; set; }
    }
}