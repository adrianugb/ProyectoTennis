using AcademiaTennisDAL;
using AcademiaTennisDAL.Context;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using System.Diagnostics;

namespace ProyectoGrupalTennis.Controllers
{
    public class HomeController : Controller
    {
        private readonly AcademiaTennisDAL.Context.AppDbContext _context;

        public HomeController(AcademiaTennisDAL.Context.AppDbContext context)
        {
            _context = context;
        }
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

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Dashboard(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var model = await ConstruirDashboardAdminAsync(fechaInicio, fechaFin);
            return View("~/Views/Dashboard/Index.cshtml", model);
        }

        // ADM-08-004: recarga solo el bloque de administrador vía AJAX al aplicar el filtro de fechas
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> FiltrarDashboardAdmin(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var model = await ConstruirDashboardAdminAsync(fechaInicio, fechaFin);
            return PartialView("~/Views/Dashboard/_DashboardAdmin.cshtml", model);
        }

        private async Task<DashboardAdminViewModel> ConstruirDashboardAdminAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // ADM-08-004: si no mandan fechas, usamos el mes actual por defecto
            var inicio = (fechaInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).Date;
            var fin = (fechaFin ?? inicio.AddMonths(1).AddDays(-1)).Date;

            // ADM-08-002: alumnos activos = alumnos con al menos una matrícula "Activa"
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

            // ADM-08-003: clases programadas dentro del rango de fechas seleccionado
            var clasesDelPeriodo = await _context.ClasesProgramadas
                .Include(c => c.Curso)
                    .ThenInclude(c => c.Profesor)
                .Where(c => c.FechaClase.Date >= inicio && c.FechaClase.Date <= fin)
                .OrderBy(c => c.FechaClase)
                .ToListAsync();

            // Profesores activos (indicador general del panel)
            var profesoresActivos = await _context.Profesores
                .Where(p => p.Activo)
                .CountAsync();

            // ADM-08-001: cursos mas demandados (con mas matriculas) en el periodo
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

            // Ingresos del periodo (pagos con estado "Pagado")
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

            // Alertas simples (puedes ampliarlas después)
            var clasesLlenas = model.ClasesDelPeriodo.Count(c => c.CuposOcupados >= c.CuposTotales && c.CuposTotales > 0);
            if (clasesLlenas > 0)
                model.Alertas.Add($"{clasesLlenas} clase(s) alcanzaron el límite de cupos.");

            if (alumnosNuevosEnPeriodo > 0)
                model.Alertas.Add($"{alumnosNuevosEnPeriodo} alumno(s) nuevo(s) se matricularon en este periodo.");

            return model;
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

        [Authorize(Roles = "Administrador")]
        public IActionResult AdminPagos()
        {
            return RedirectToAction("AdminPagos", "Admin");
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
            return RedirectToAction("HistorialPagos", "Usuario");
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