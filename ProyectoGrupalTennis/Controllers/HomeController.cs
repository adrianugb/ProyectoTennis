using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;
using System.Diagnostics;

namespace ProyectoGrupalTennis.Controllers
{
    public class HomeController : Controller
    {
        #region Vistas generales

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion

        #region Dashboard

        public IActionResult Dashboard()
        {
            return View("~/Views/Dashboard/Index.cshtml");
        }

        #endregion

        #region Notificaciones

        public IActionResult Notificaciones()
        {
            return View("~/Views/Notificaciones/Index.cshtml");
        }

        #endregion

        #region Feedback

        public IActionResult Feedback()
        {
            return View("~/Views/Feedback/Index.cshtml");
        }

        #endregion

        #region Progreso

        public IActionResult Progreso()
        {
            return View("~/Views/Progreso/Index.cshtml");
        }

        #endregion

        #region Seguridad y autenticación

        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        #endregion
    }
}