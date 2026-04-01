using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;
using System.Diagnostics;

namespace ProyectoGrupalTennis.Controllers
{
    public class HomeController : Controller
    {
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
        public IActionResult Dashboard()
        {
            return View("~/Views/Dashboard/Index.cshtml");
        }

        public IActionResult Notificaciones()
        {
            return View("~/Views/Notificaciones/Index.cshtml");
        }

        public IActionResult Feedback()
        {
            return View("~/Views/Feedback/Index.cshtml");
        }

        public IActionResult Progreso()
        {
            return View("~/Views/Progreso/Index.cshtml");
        }
    }
}
