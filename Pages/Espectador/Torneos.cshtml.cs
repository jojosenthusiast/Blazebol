using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;
using ControlTorneosFutbol.Helpers;

namespace ControlTorneosFutbol.Pages.Espectador
{
    public class TorneosModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TorneosModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TorneoItemViewModel> Torneos { get; set; } = new();
        public string? FiltroEstado { get; set; }

        public async Task<IActionResult> OnGetAsync(string? estado)
        {
            if (!HttpContext.Session.IsAuthenticated() || !HttpContext.Session.IsInRole("Espectador"))
                return RedirectToPage("/Account/Login");

            FiltroEstado = estado;

            // Espectadores nunca ven torneos en Borrador
            var query = _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .Where(t => t.Estado != EstadosTorneo.Borrador)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado) && estado != EstadosTorneo.Borrador)
                query = query.Where(t => t.Estado == estado);

            var torneos = await query
                .OrderByDescending(t => t.Estado == EstadosTorneo.Activo)
                .ThenByDescending(t => t.FechaInicio)
                .ToListAsync();

            Torneos = torneos.Select(t => new TorneoItemViewModel
            {
                Id = t.Id,
                Nombre = t.Nombre,
                NombreLiga = t.Liga.Nombre,
                NombreTemporada = t.Temporada.Nombre,
                NombreCategoria = t.Categoria.Nombre,
                Formato = t.Formato,
                Estado = t.Estado,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin
            }).ToList();

            return Page();
        }
    }
}
