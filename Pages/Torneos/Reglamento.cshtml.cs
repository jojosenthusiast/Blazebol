using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Services;

namespace ControlTorneosFutbol.Pages.Torneos
{
    public class ReglamentoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public ReglamentoModel(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var torneo = await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Categoria)
                .Include(t => t.ConfiguracionTorneo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (torneo == null)
                return NotFound();

            var pdf = _pdfService.GenerarReglamentoPdf(torneo, torneo.ConfiguracionTorneo);
            var nombreArchivo = $"Reglamento_{torneo.Nombre.Replace(" ", "_")}.pdf";
            return File(pdf, "application/pdf", nombreArchivo);
        }
    }
}
