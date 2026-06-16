using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

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

        public IActionResult Retencion()
        {
            return View();
        }
        public IActionResult Campeonatos()
        {
            return View("~/Views/Campeonatos/campeonatos.cshtml");
        }

        public IActionResult Tienda()
        {
            return View("~/Views/Home/Tienda.cshtml");
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

        public IActionResult Registro()
        {
            return RedirectToAction("Registro", "Auth");
        }

        public IActionResult RecuperarContrasena()
        {
            return View("~/Views/Auth/RecuperarContrasena.cshtml");
        }

        #endregion

        #region Perfiles

       
        public IActionResult PerfilAdmin()
        {
            return View("~/Views/Perfiles/PerfilAdmin.cshtml");
        }

        public IActionResult AdminCursos()
        {
            return RedirectToAction("Index", "Curso");
        }

        public IActionResult AdminAlumnos()
        {
            return RedirectToAction("Index", "Alumnos");
        }

        public IActionResult AdminProfesores()
        {
            return View("~/Views/Perfiles/AdminProfesores.cshtml");
        }

        public IActionResult AdminFacturacion()
        {
            return View("~/Views/Perfiles/AdminFacturacion.cshtml");
        }

        public IActionResult AdminPagos()
        {
            return View("~/Views/Perfiles/AdminPagos.cshtml");
        }

        public IActionResult AdminFacturas()
        {
            return View("~/Views/Perfiles/AdminFacturas.cshtml");
        }

        public IActionResult AdminUsuario()
        {
            return View("~/Views/Perfiles/AdminUsuario.cshtml");
        }
        #endregion

        #region Perfil Usuario 

        
        public IActionResult PerfilUsuario()
        {
            return View("~/Views/Perfiles/PerfilUsuario.cshtml");
        }
        public IActionResult UsuarioPagos()
        {
            return View("~/Views/Perfiles/UsuarioPagos.cshtml");
        }

        public IActionResult UsuarioHistorialPagos()
        {
            return View("~/Views/Perfiles/UsuarioHistorialPagos.cshtml");
        }

        #endregion

        #region Perfil Profesor

        public IActionResult PerfilProfesor()
        {
            return View("~/Views/Perfiles/PerfilProfesor.cshtml");
        }

        public IActionResult ProfesorAlumnos()
        {
            return View("~/Views/Perfiles/ProfesorAlumnos.cshtml");
        }

       public IActionResult ProfesorCursos()
        {
            return View("~/Views/Perfiles/ProfesorCursos.cshtml");
        }
        #endregion

        #region Gamificacion

        public IActionResult Gamificacion()
        {
            return View("~/Views/Gamificacion/Index.cshtml");
        }

        #endregion

        #region Geolocalizacion

        public IActionResult Geolocalizacion()
        {
            return View("~/Views/Geolocalizacion/Index.cshtml");
        }

        #endregion

        #region Matricula

        public IActionResult Matricula()
        {
            return View("~/Views/Matricula/Index.cshtml");
        }

        #endregion
    }
}