using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using System.Security.Claims;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly AppDbContext _context;

        public NotificacionesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Usuario,Administrador")]
        public async Task<IActionResult> Index()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var notificaciones = await _context.Notificaciones
                .Where(n => n.IdUsuario == userId)
                .OrderByDescending(n => n.FechaEnvio)
                .ToListAsync();

            var preferencia = await _context.PreferenciasNotificacion
                .FirstOrDefaultAsync(p => p.IdUsuario == userId);

            var model = new NotificacionesUsuarioViewModel
            {
                Notificaciones = notificaciones.Select(n =>
                    new NotificacionUsuarioItemViewModel
                    {
                        IdNotificacion = n.IdNotificacion,
                        Tipo = n.Tipo,
                        Titulo = n.Titulo,
                        Mensaje = n.Mensaje,
                        Leida = n.Leida,
                        FechaEnvio = n.FechaEnvio
                    }).ToList(),

                CanalPreferido =
                    preferencia?.CanalPreferido ?? "Email",

                NotificacionesPago =
                    preferencia?.NotificacionesPago ?? true,

                NotificacionesClase =
                    preferencia?.NotificacionesClase ?? true,

                NotificacionesRecordatorio =
                    preferencia?.NotificacionesRecordatorio ?? true,

                NotificacionesCampeonato =
                    preferencia?.NotificacionesCampeonato ?? true
            };

            return View(
                "~/Views/Notificaciones/_NotificacionesUsuario.cshtml",
                model);
        }

        [HttpPost]
        [Authorize(Roles = "Usuario,Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarLeida(
            int idNotificacion)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n =>
                    n.IdNotificacion == idNotificacion &&
                    n.IdUsuario == userId);

            if (notificacion == null)
            {
                TempData["Error"] =
                    "No se encontró la notificación seleccionada.";

                return RedirectToAction(nameof(Index));
            }

            notificacion.Leida = true;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "La notificación fue marcada como leída.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Usuario,Administrador")]
        public async Task<IActionResult> Resumen()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var noLeidas = await _context.Notificaciones
                .CountAsync(n =>
                    n.IdUsuario == userId &&
                    !n.Leida);

            var recientes = await _context.Notificaciones
                .Where(n => n.IdUsuario == userId)
                .OrderByDescending(n => n.FechaEnvio)
                .Take(8)
                .Select(n => new
                {
                    id = n.IdNotificacion,
                    titulo = n.Titulo,
                    mensaje = n.Mensaje,
                    leida = n.Leida,
                    fecha = n.FechaEnvio
                        .ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new
            {
                noLeidas,
                notificaciones = recientes
            });
        }

        [HttpPost]
        [Authorize(Roles = "Usuario,Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(
            int idNotificacion)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n =>
                    n.IdNotificacion == idNotificacion &&
                    n.IdUsuario == userId);

            var esAjax =
                Request.Headers["X-Requested-With"]
                == "XMLHttpRequest";

            if (notificacion == null)
            {
                if (esAjax)
                {
                    return Json(new
                    {
                        success = false,
                        mensaje =
                            "No se encontró la notificación."
                    });
                }

                TempData["Error"] =
                    "No se encontró la notificación seleccionada.";

                return RedirectToAction(nameof(Index));
            }

            _context.Notificaciones.Remove(notificacion);

            await _context.SaveChangesAsync();

            if (esAjax)
            {
                return Json(new
                {
                    success = true
                });
            }

            TempData["MensajeExito"] =
                "La notificación fue eliminada.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Usuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPreferencias(
            string canalPreferido,
            bool notificacionesPago,
            bool notificacionesClase,
            bool notificacionesRecordatorio,
            bool notificacionesCampeonato)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var preferencia =
                await _context.PreferenciasNotificacion
                    .FirstOrDefaultAsync(p =>
                        p.IdUsuario == userId);

            if (preferencia == null)
            {
                preferencia = new PreferenciaNotificacion
                {
                    IdUsuario = userId
                };

                _context.PreferenciasNotificacion
                    .Add(preferencia);
            }

            preferencia.CanalPreferido =
                canalPreferido;

            preferencia.NotificacionesPago =
                notificacionesPago;

            preferencia.NotificacionesClase =
                notificacionesClase;

            preferencia.NotificacionesRecordatorio =
                notificacionesRecordatorio;

            preferencia.NotificacionesCampeonato =
                notificacionesCampeonato;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Tus preferencias de notificaciones fueron actualizadas.";

            return RedirectToAction(nameof(Index));
        }
    }
}