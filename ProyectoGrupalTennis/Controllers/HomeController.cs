using AcademiaTennisDAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;
using System.Diagnostics;

namespace ProyectoGrupalTennis.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly GoogleCalendarService _calendarService;

        public HomeController(AppDbContext context, GoogleCalendarService calendarService)
        {
            _context = context;
            _calendarService = calendarService;
        }

        #region Vistas generales

        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        // Módulo desactivado
        //public IActionResult Retencion() => View();

        public IActionResult Campeonatos() =>
            View("~/Views/Campeonatos/campeonatos.cshtml");

        public IActionResult Tienda() =>
            View("~/Views/Home/Tienda.cshtml");

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        #endregion

        #region Dashboard

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Dashboard(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var model = await ConstruirDashboardAdminAsync(fechaInicio, fechaFin);
            return View("~/Views/Dashboard/Index.cshtml", model);
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> FiltrarDashboardAdmin(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var model = await ConstruirDashboardAdminAsync(fechaInicio, fechaFin);
            return PartialView("~/Views/Dashboard/_DashboardAdmin.cshtml", model);
        }

        private async Task<DashboardAdminViewModel> ConstruirDashboardAdminAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = (fechaInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).Date;
            var fin = (fechaFin ?? inicio.AddMonths(1).AddDays(-1)).Date;

            var alumnosActivos = await _context.Matriculas
                .Where(m => m.Estado == "Activa")
                .Select(m => m.IdAlumno)
                .Distinct()
                .CountAsync();

            var alumnosNuevosEnPeriodo = await _context.Matriculas
                .Where(m => m.Estado == "Activa"
                         && m.FechaMatricula.Date >= inicio
                         && m.FechaMatricula.Date <= fin)
                .Select(m => m.IdAlumno)
                .Distinct()
                .CountAsync();

            var clasesDelPeriodo = await _context.ClasesProgramadas
                .Include(c => c.Curso)
                    .ThenInclude(c => c.Profesor)
                .Where(c => c.FechaClase.Date >= inicio && c.FechaClase.Date <= fin)
                .OrderBy(c => c.FechaClase)
                .ToListAsync();

            var profesoresActivos = await _context.Profesores
                .Where(p => p.Activo)
                .CountAsync();

            var cursosMasDemandados = await _context.Matriculas
                .Where(m => m.FechaMatricula.Date >= inicio && m.FechaMatricula.Date <= fin)
                .GroupBy(m => m.Curso.Nombre)
                .Select(g => new CursoDemandaViewModel
                {
                    NombreCurso = g.Key,
                    TotalMatriculas = g.Count()
                })
                .OrderByDescending(c => c.TotalMatriculas)
                .Take(3)
                .ToListAsync();

            var ingresosPeriodo = await _context.Pagos
                .Where(p => p.Estado == "Pagado"
                         && p.FechaPago.Date >= inicio
                         && p.FechaPago.Date <= fin)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var model = new DashboardAdminViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin,
                AlumnosActivos = alumnosActivos,
                AlumnosNuevosEnPeriodo = alumnosNuevosEnPeriodo,
                ClasesProgramadas = clasesDelPeriodo.Count,
                ProfesoresActivos = profesoresActivos,
                IngresosPeriodo = ingresosPeriodo,
                CursosMasDemandados = cursosMasDemandados,
                ClasesDelPeriodo = clasesDelPeriodo.Select(c => new ClaseResumenViewModel
                {
                    NombreCurso = c.Curso?.Nombre ?? "Sin curso",
                    Profesor = c.Curso?.Profesor != null
                        ? $"{c.Curso.Profesor.Nombre} {c.Curso.Profesor.Apellidos}"
                        : "Sin asignar",
                    Fecha = c.FechaClase,
                    HoraInicio = c.HoraInicio,
                    CuposOcupados = c.Curso?.Matriculas?.Count(m => m.Estado == "Activa") ?? 0,
                    CuposTotales = c.Curso?.CuposDisponibles ?? 0,
                    Estado = c.Estado
                }).ToList(),
                Alertas = new List<string>()
            };

            var clasesLlenas = model.ClasesDelPeriodo.Count(c => c.CuposOcupados >= c.CuposTotales && c.CuposTotales > 0);
            if (clasesLlenas > 0)
                model.Alertas.Add($"{clasesLlenas} clase(s) alcanzaron el límite de cupos.");

            if (alumnosNuevosEnPeriodo > 0)
                model.Alertas.Add($"{alumnosNuevosEnPeriodo} alumno(s) nuevo(s) se matricularon en este periodo.");

            return model;
        }

        #endregion

        #region Notificaciones

        public IActionResult Notificaciones() =>
            View("~/Views/Notificaciones/Index.cshtml");

        #endregion

        #region Feedback

        public IActionResult Feedback() =>
            View("~/Views/Feedback/Index.cshtml");

        #endregion

        #region Progreso

        public IActionResult Progreso() =>
            View("~/Views/Progreso/Index.cshtml");

        #endregion

        #region Seguridad y autenticación

        public IActionResult Login() =>
            View("~/Views/Auth/Login.cshtml");

        public IActionResult Registro() =>
            RedirectToAction("Registro", "Auth");

        public IActionResult RecuperarContrasena() =>
            View("~/Views/Auth/RecuperarContrasena.cshtml");

        #endregion

        #region Perfiles

        public IActionResult PerfilAdmin() =>
            View("~/Views/Perfiles/PerfilAdmin.cshtml");

        public IActionResult AdminCursos() =>
            RedirectToAction("Index", "Curso");

        public IActionResult AdminAlumnos() =>
            RedirectToAction("Index", "Alumnos");

        public IActionResult AdminProfesores() =>
            View("~/Views/Perfiles/AdminProfesores.cshtml");

        public IActionResult AdminFacturacion() =>
            View("~/Views/Perfiles/AdminFacturacion.cshtml");

        [Authorize(Roles = "Administrador")]
        public IActionResult AdminPagos() =>
            RedirectToAction("AdminPagos", "Admin");

        public IActionResult AdminFacturas() =>
            View("~/Views/Perfiles/AdminFacturas.cshtml");

        public IActionResult AdminUsuario() =>
            View("~/Views/Perfiles/AdminUsuario.cshtml");

        #endregion

        #region Perfil Usuario

        public IActionResult PerfilUsuario() =>
            View("~/Views/Perfiles/PerfilUsuario.cshtml");

        public IActionResult UsuarioPagos() =>
            RedirectToAction("HistorialPagos", "Usuario");

        public IActionResult UsuarioHistorialPagos() =>
            View("~/Views/Perfiles/UsuarioHistorialPagos.cshtml");

        #endregion

        #region Perfil Profesor

        public async Task<IActionResult> PerfilProfesor()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            ViewBag.TieneGoogleCalendar = userId != null
                && await _calendarService.TieneTokenAsync(userId);
            return View("~/Views/Perfiles/PerfilProfesor.cshtml");
        }

        public IActionResult ProfesorAlumnos() =>
            View("~/Views/Perfiles/ProfesorAlumnos.cshtml");

        public IActionResult ProfesorCursos() =>
            RedirectToAction("MisCursos", "PerfilProfesor");

        #endregion

        #region Gamificacion

        // Módulo desactivado a pedido del cliente (USER-DESACTIVAR-01)
        //public IActionResult Gamificacion() =>
        //    View("~/Views/Gamificacion/Index.cshtml");

        #endregion

        #region Geolocalizacion

        public IActionResult Geolocalizacion() =>
            View("~/Views/Geolocalizacion/Index.cshtml");

        #endregion

        #region Matricula

        public IActionResult Matricula() =>
            View("~/Views/Matricula/Index.cshtml");

        #endregion
    }
}