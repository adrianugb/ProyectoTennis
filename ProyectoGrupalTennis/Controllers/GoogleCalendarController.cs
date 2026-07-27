using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Services;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize]
    public class GoogleCalendarController : Controller
    {
        private readonly GoogleCalendarService _calendarService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GoogleCalendarController(
            GoogleCalendarService calendarService,
            UserManager<ApplicationUser> userManager)
        {
            _calendarService = calendarService;
            _userManager = userManager;
        }

        // GET: /GoogleCalendar/Autorizar
        // Redirige al usuario a la pantalla de autorización de Google
        public IActionResult Autorizar(string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User)!;
            // Guardamos returnUrl en sesión para redirigir después del callback
            HttpContext.Session.SetString("CalendarReturnUrl", returnUrl ?? "/");
            var url = _calendarService.ObtenerUrlAutorizacion(userId);
            return Redirect(url);
        }

        // GET: /GoogleCalendar/Callback
        // Google redirige aquí con el código de autorización
        public async Task<IActionResult> Callback(string code, string state)
        {
            // state contiene el userId que pusimos al generar la URL
            await _calendarService.GuardarTokenAsync(state, code);

            TempData["MensajeExito"] = "Google Calendar conectado correctamente.";

            var returnUrl = HttpContext.Session.GetString("CalendarReturnUrl") ?? "/";
            return Redirect(returnUrl);
        }

        // GET: /GoogleCalendar/Desconectar
        public async Task<IActionResult> Desconectar()
        {
            var userId = _userManager.GetUserId(User)!;
            var token = await _calendarService.ObtenerTokenAsync(userId);

            if (token != null)
            {
                await _calendarService.EliminarTokenAsync(userId);
                TempData["MensajeExito"] = "Google Calendar desconectado.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
