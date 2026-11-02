using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models.ViewModels;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionController : Controller
    {
        private readonly AppDbContext _context;

        public ConfiguracionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Configuracion
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var configuracion = await _context.ConfiguracionesAcademia
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == 1);

            var modelo = new ConfiguracionAcademiaViewModel();

            if (configuracion != null)
            {
                modelo.WhatsappSolicitudes =
                    configuracion.WhatsappSolicitudes;

                modelo.CorreoSolicitudes =
                    configuracion.CorreoSolicitudes;

                modelo.SmtpEmail =
                    configuracion.SmtpEmail;

                modelo.SmtpServer =
                    configuracion.SmtpServer;

                modelo.SmtpPort =
                    configuracion.SmtpPort;

                modelo.SmtpSenderName =
                    configuracion.SmtpSenderName;
            }

            return View(modelo);
        }

        // POST: /Configuracion/Guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(
            ConfiguracionAcademiaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", modelo);
            }

            var configuracion = await _context.ConfiguracionesAcademia
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (configuracion == null)
            {
                configuracion = new ConfiguracionAcademia
                {
                    Id = 1
                };

                _context.ConfiguracionesAcademia.Add(configuracion);
            }

            configuracion.WhatsappSolicitudes =
                modelo.WhatsappSolicitudes.Trim();

            configuracion.CorreoSolicitudes =
                modelo.CorreoSolicitudes.Trim();

            configuracion.SmtpEmail =
                modelo.SmtpEmail.Trim();

            configuracion.SmtpServer =
                modelo.SmtpServer.Trim();

            configuracion.SmtpPort =
                modelo.SmtpPort;

            configuracion.SmtpSenderName =
                modelo.SmtpSenderName.Trim();

            configuracion.FechaActualizacion =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "La configuración se guardó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}