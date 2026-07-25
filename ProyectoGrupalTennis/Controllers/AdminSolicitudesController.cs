using AcademiaTennisDAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminSolicitudesController : Controller
    {
        private readonly AppDbContext _context;

        public AdminSolicitudesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesCurso
                .Include(s => s.Alumno)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.Alumno)
                .Include(s => s.TarifaClase)
                    .ThenInclude(t => t.TipoClase)
                .Include(s => s.Disponibilidades)
                .FirstOrDefaultAsync(s => s.IdSolicitudCurso == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarEnRevision(int id)
        {
            var solicitud = await _context.SolicitudesCurso
                .FirstOrDefaultAsync(s => s.IdSolicitudCurso == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Pendiente")
            {
                TempData["Error"] =
                    "La solicitud ya no se encuentra en estado Pendiente.";

                return RedirectToAction(
                    nameof(Detalle),
                    new { id });
            }

            solicitud.Estado = "En revisión";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La solicitud fue marcada como En revisión.";

            return RedirectToAction(
                nameof(Detalle),
                new { id });
        }
    }
}