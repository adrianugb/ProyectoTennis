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
        private readonly ILogger<GoogleCalendarController> _logger;

        public GoogleCalendarController(
            GoogleCalendarService calendarService,
            UserManager<ApplicationUser> userManager,
            ILogger<GoogleCalendarController> logger)
        {
            _calendarService = calendarService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /GoogleCalendar/Autorizar
        public IActionResult Autorizar(string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User)!;

            // Guardamos returnUrl en TempData en vez de Session
            // TempData es más confiable en Azure que Session
            TempData["CalendarReturnUrl"] = returnUrl ?? "/Home/PerfilProfesor";

            _logger.LogInformation("Iniciando autorización Google Calendar para userId: {UserId}", userId);

            var url = _calendarService.ObtenerUrlAutorizacion(userId);
            return Redirect(url);
        }

        // GET: /GoogleCalendar/Callback
        public async Task<IActionResult> Callback(string? code, string? state, string? error)
        {
            // Si Google devuelve error (ej: usuario canceló)
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Google Calendar OAuth error: {Error}", error);
                TempData["MensajeError"] = $"No se pudo conectar Google Calendar: {error}";
                return RedirectToAction("PerfilProfesor", "Home");
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogWarning("Callback recibido sin code o state");
                TempData["MensajeError"] = "Error en la autorización de Google Calendar.";
                return RedirectToAction("PerfilProfesor", "Home");
            }

            try
            {
                _logger.LogInformation("Guardando token para userId: {State}", state);
                await _calendarService.GuardarTokenAsync(state, code);
                _logger.LogInformation("Token guardado correctamente para userId: {State}", state);

                TempData["MensajeExito"] = "Google Calendar conectado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar token de Google Calendar");
                TempData["MensajeError"] = "Error al conectar Google Calendar. Intentá nuevamente.";
            }

            var returnUrl = TempData["CalendarReturnUrl"]?.ToString() ?? "/Home/PerfilProfesor";
            return Redirect(returnUrl);
        }

        // GET: /GoogleCalendar/Desconectar
        public async Task<IActionResult> Desconectar()
        {
            var userId = _userManager.GetUserId(User)!;

            try
            {
                await _calendarService.EliminarTokenAsync(userId);
                TempData["MensajeExito"] = "Google Calendar desconectado.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desconectar Google Calendar");
                TempData["MensajeError"] = "Error al desconectar Google Calendar.";
            }

            return RedirectToAction("PerfilProfesor", "Home");
        }
    }
}