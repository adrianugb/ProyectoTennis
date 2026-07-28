using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize]
    public class ProgresoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProgresoController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =====================================================
        // REDIRECCIÓN SEGÚN EL ROL
        // =====================================================

        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("Administrador"))
            {
                return RedirectToAction(nameof(AdminIndex));
            }

            if (User.IsInRole("Profesor"))
            {
                return RedirectToAction(nameof(ProfesorIndex));
            }

            if (User.IsInRole("Usuario"))
            {
                return RedirectToAction(nameof(AlumnoIndex));
            }

            return Forbid();
        }

        // =====================================================
        // PROFESOR
        // =====================================================

        [Authorize(Roles = "Profesor")]
        [HttpGet]
        public async Task<IActionResult> ProfesorIndex()
        {
            var profesorActual = await _userManager.GetUserAsync(User);

            if (profesorActual == null)
            {
                return Unauthorized();
            }

            ViewBag.TotalEvaluaciones = await _context.ProgresosAlumno
                .CountAsync(p => p.IdProfesor == profesorActual.Id);

            ViewBag.TotalAlumnosEvaluados = await _context.ProgresosAlumno
                .Where(p => p.IdProfesor == profesorActual.Id)
                .Select(p => p.IdAlumno)
                .Distinct()
                .CountAsync();

            ViewBag.UltimaEvaluacion = await _context.ProgresosAlumno
                .Where(p => p.IdProfesor == profesorActual.Id)
                .OrderByDescending(p => p.FechaEvaluacion)
                .Select(p => (DateTime?)p.FechaEvaluacion)
                .FirstOrDefaultAsync();

            return View("ProfesorIndex");
        }

        // =====================================================
        // REGISTRAR PROGRESO - GET
        // =====================================================

        [Authorize(Roles = "Profesor")]
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            var modelo = new RegistrarProgresoViewModel();

            await CargarListas(modelo);

            return View("Registrar", modelo);
        }

        // =====================================================
        // REGISTRAR PROGRESO - POST
        // =====================================================

        [Authorize(Roles = "Profesor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistrarProgresoViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                await CargarListas(modelo);
                return View("Registrar", modelo);
            }

            var profesorActual = await _userManager.GetUserAsync(User);

            if (profesorActual == null)
            {
                return Unauthorized();
            }

            var alumnoExiste = await _context.Users
                .AnyAsync(u => u.Id == modelo.IdAlumno);

            if (!alumnoExiste)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdAlumno),
                    "El alumno seleccionado no existe.");

                await CargarListas(modelo);
                return View("Registrar", modelo);
            }

            var cursoExiste = await _context.Cursos
                .AnyAsync(c =>
                    c.IdCurso == modelo.IdCurso &&
                    c.Activo);

            if (!cursoExiste)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdCurso),
                    "El curso seleccionado no existe o está inactivo.");

                await CargarListas(modelo);
                return View("Registrar", modelo);
            }
            var matricula = await _context.Matriculas
                .FirstOrDefaultAsync(m =>
                    m.IdCurso == modelo.IdCurso &&
                    m.IdAlumno == modelo.IdAlumno);

            if (matricula == null)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdAlumno),
                    "El alumno seleccionado no está matriculado en este curso.");

                await CargarListas(modelo);
                return View("Registrar", modelo);
            }

            var pagoConfirmado = await _context.Pagos
                .AnyAsync(p =>
                    p.IdMatricula == matricula.IdMatricula &&
                    p.Estado == "Pagado");

            if (!pagoConfirmado)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdAlumno),
                    "El alumno no tiene el pago confirmado para este curso.");

                await CargarListas(modelo);
                return View("Registrar", modelo);
            }

            var fechaActual = DateTime.Now;

            var horariosCurso = await _context.Horarios
                .Where(h => h.IdCurso == modelo.IdCurso)
                .ToListAsync();

            if (!horariosCurso.Any())
            {
                ModelState.AddModelError(
                    nameof(modelo.IdCurso),
                    "El curso no tiene horarios registrados.");

                await CargarListas(modelo);
                return View("Registrar", modelo);
            }

            var existeClaseFinalizadaHoy = horariosCurso.Any(h =>
            {
                var diaHorario = ConvertirDiaSemana(h.DiaSemana);

                if (diaHorario == null)
                {
                    return false;
                }

                bool esHoy =
                    fechaActual.DayOfWeek == diaHorario.Value;

                bool yaTermino =
                    fechaActual.TimeOfDay >= h.HoraFin;

                return esHoy && yaTermino;
            });

            if (!existeClaseFinalizadaHoy)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdCurso),
                    "La evaluación solo puede registrarse después de finalizar la clase de hoy.");

                await CargarListas(modelo);
                return View("Registrar", modelo);
            }


            var progreso = new ProgresoAlumno
            {
                IdAlumno = modelo.IdAlumno,
                IdProfesor = profesorActual.Id,
                IdCurso = modelo.IdCurso,

                NivelSaque = modelo.NivelSaque,
                NivelReves = modelo.NivelReves,
                NivelDerecha = modelo.NivelDerecha,
                NivelVolea = modelo.NivelVolea,
                NivelMovimiento = modelo.NivelMovimiento,
                NivelTactica = modelo.NivelTactica,

                Observaciones = modelo.Observaciones,
                AreasMejora = modelo.AreasMejora,

                FechaEvaluacion = DateTime.Now
            };

            progreso.NivelGeneral =
                CalcularNivelGeneral(progreso);

            _context.ProgresosAlumno.Add(progreso);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"La evaluación fue registrada correctamente. " +
                $"Nivel general: {progreso.NivelGeneral}.";

            return RedirectToAction(nameof(ProfesorIndex));
        }

        // =====================================================
        // HISTORIAL DEL PROFESOR
        // =====================================================

        [Authorize(Roles = "Profesor")]
        [HttpGet]
        public async Task<IActionResult> ProfesorHistorial(
            string? buscar,
            int? idCurso)
        {
            var profesorActual = await _userManager.GetUserAsync(User);

            if (profesorActual == null)
            {
                return Unauthorized();
            }

            var consulta = _context.ProgresosAlumno
                .Include(p => p.Alumno)
                .Include(p => p.Curso)
                .Where(p => p.IdProfesor == profesorActual.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(p =>
                    p.Alumno.Nombre.Contains(buscar) ||
                    p.Alumno.Apellido.Contains(buscar));
            }

            if (idCurso.HasValue)
            {
                consulta = consulta.Where(
                    p => p.IdCurso == idCurso.Value);
            }

            var progresos = await consulta
                .OrderByDescending(p => p.FechaEvaluacion)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.IdCurso = idCurso;

            ViewBag.Cursos = await _context.Cursos
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.IdCurso.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();

            return View("ProfesorHistorial", progresos);
        }

        // =====================================================
        // EDITAR PROGRESO - GET
        // =====================================================

        [Authorize(Roles = "Profesor")]
        [HttpGet]
        public async Task<IActionResult> EditarProgreso(int id)
        {
            var profesorActual = await _userManager.GetUserAsync(User);

            if (profesorActual == null)
            {
                return Unauthorized();
            }

            var progreso = await _context.ProgresosAlumno
                .FirstOrDefaultAsync(p =>
                    p.IdProgreso == id &&
                    p.IdProfesor == profesorActual.Id);

            if (progreso == null)
            {
                return NotFound();
            }

            var modelo = new RegistrarProgresoViewModel
            {
                IdAlumno = progreso.IdAlumno,
                IdCurso = progreso.IdCurso,

                NivelSaque = progreso.NivelSaque,
                NivelReves = progreso.NivelReves,
                NivelDerecha = progreso.NivelDerecha,
                NivelVolea = progreso.NivelVolea,
                NivelMovimiento = progreso.NivelMovimiento,
                NivelTactica = progreso.NivelTactica,

                Observaciones = progreso.Observaciones,
                AreasMejora = progreso.AreasMejora
            };

            await CargarListas(modelo);

            ViewBag.IdProgreso = progreso.IdProgreso;
            ViewBag.FechaEvaluacion = progreso.FechaEvaluacion;

            return View("EditarProgreso", modelo);
        }

        // =====================================================
        // EDITAR PROGRESO - POST
        // =====================================================

        [Authorize(Roles = "Profesor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProgreso(
            int id,
            RegistrarProgresoViewModel modelo)
        {
            var profesorActual = await _userManager.GetUserAsync(User);

            if (profesorActual == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                await CargarListas(modelo);

                ViewBag.IdProgreso = id;

                return View("EditarProgreso", modelo);
            }

            var progreso = await _context.ProgresosAlumno
                .FirstOrDefaultAsync(p =>
                    p.IdProgreso == id &&
                    p.IdProfesor == profesorActual.Id);

            if (progreso == null)
            {
                return NotFound();
            }

            var alumnoExiste = await _context.Users
                .AnyAsync(u => u.Id == modelo.IdAlumno);

            if (!alumnoExiste)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdAlumno),
                    "El alumno seleccionado no existe.");

                await CargarListas(modelo);

                ViewBag.IdProgreso = id;

                return View("EditarProgreso", modelo);
            }

            var cursoExiste = await _context.Cursos
                .AnyAsync(c => c.IdCurso == modelo.IdCurso);

            if (!cursoExiste)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdCurso),
                    "El curso seleccionado no existe.");

                await CargarListas(modelo);

                ViewBag.IdProgreso = id;

                return View("EditarProgreso", modelo);
            }

            progreso.IdAlumno = modelo.IdAlumno;
            progreso.IdCurso = modelo.IdCurso;

            progreso.NivelSaque = modelo.NivelSaque;
            progreso.NivelReves = modelo.NivelReves;
            progreso.NivelDerecha = modelo.NivelDerecha;
            progreso.NivelVolea = modelo.NivelVolea;
            progreso.NivelMovimiento = modelo.NivelMovimiento;
            progreso.NivelTactica = modelo.NivelTactica;

            progreso.Observaciones = modelo.Observaciones;
            progreso.AreasMejora = modelo.AreasMejora;

            progreso.NivelGeneral =
                CalcularNivelGeneral(progreso);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La evaluación fue actualizada correctamente.";

            return RedirectToAction(nameof(ProfesorHistorial));
        }

        // =====================================================
        // ALUMNO
        // =====================================================

        [Authorize(Roles = "Usuario")]
        [HttpGet]
        public async Task<IActionResult> AlumnoIndex()
        {
            var alumnoActual = await _userManager.GetUserAsync(User);

            if (alumnoActual == null)
            {
                return Unauthorized();
            }

            var progresos = await _context.ProgresosAlumno
                .Include(p => p.Profesor)
                .Include(p => p.Curso)
                .Where(p => p.IdAlumno == alumnoActual.Id)
                .OrderByDescending(p => p.FechaEvaluacion)
                .ToListAsync();

            return View("AlumnoIndex", progresos);
        }

        // =====================================================
        // ADMINISTRADOR
        // =====================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> AdminIndex()
        {
            ViewBag.TotalEvaluaciones =
                await _context.ProgresosAlumno.CountAsync();

            ViewBag.TotalAlumnosEvaluados =
                await _context.ProgresosAlumno
                    .Select(p => p.IdAlumno)
                    .Distinct()
                    .CountAsync();

            ViewBag.TotalProfesoresEvaluadores =
                await _context.ProgresosAlumno
                    .Where(p => p.IdProfesor != null)
                    .Select(p => p.IdProfesor)
                    .Distinct()
                    .CountAsync();

            ViewBag.TotalPrincipiantes =
                await _context.ProgresosAlumno
                    .CountAsync(p =>
                        p.NivelGeneral == "Principiante");

            ViewBag.TotalIntermedios =
                await _context.ProgresosAlumno
                    .CountAsync(p =>
                        p.NivelGeneral == "Intermedio");

            ViewBag.TotalAvanzados =
                await _context.ProgresosAlumno
                    .CountAsync(p =>
                        p.NivelGeneral == "Avanzado");

            var evaluacionesRecientes =
                await _context.ProgresosAlumno
                    .Include(p => p.Alumno)
                    .Include(p => p.Profesor)
                    .Include(p => p.Curso)
                    .OrderByDescending(p => p.FechaEvaluacion)
                    .Take(10)
                    .ToListAsync();

            return View("AdminIndex", evaluacionesRecientes);
        }

        // =====================================================
        // CRITERIOS TÉCNICOS
        // =====================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Criterios()
        {
            return View("Criterios");
        }

        // =====================================================
        // NIVELES TÉCNICOS
        // =====================================================

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Niveles()
        {
            return View("Niveles");
        }

        [Authorize(Roles = "Profesor")]
        [HttpGet]
        public async Task<IActionResult> ObtenerAlumnosPorCurso(int idCurso)
        {
            var alumnos = await _context.Matriculas
                .Where(m =>
                    m.IdCurso == idCurso &&
                    m.Estado == "Activa" &&
                    m.Pagos.Any(p => p.Estado == "Pagado"))
                .Select(m => new
                {
                    id = m.IdAlumno,
                    nombre = m.Alumno.Nombre + " " + m.Alumno.Apellido
                })
                .Distinct()
                .OrderBy(a => a.nombre)
                .ToListAsync();

            return Json(alumnos);
        }

        // =====================================================
        // MÉTODOS PRIVADOS
        // =====================================================

        private static string CalcularNivelGeneral(
            ProgresoAlumno progreso)
        {
            double promedio =
                (progreso.NivelSaque +
                 progreso.NivelReves +
                 progreso.NivelDerecha +
                 progreso.NivelVolea +
                 progreso.NivelMovimiento +
                 progreso.NivelTactica) / 6.0;

            if (promedio >= 8)
            {
                return "Avanzado";
            }

            if (promedio >= 5)
            {
                return "Intermedio";
            }

            return "Principiante";
        }

        private async Task CargarListas(
            RegistrarProgresoViewModel modelo)
        {
            modelo.Alumnos = new List<SelectListItem>();

            var fechaActual = DateTime.Now;

            var cursosConHorarios = await _context.Cursos
                .Where(c => c.Activo)
                .Include(c => c.Horarios)
                .ToListAsync();

            modelo.Cursos = cursosConHorarios
                .Where(c => c.Horarios.Any(h =>
                {
                    var diaHorario = ConvertirDiaSemana(h.DiaSemana);

                    if (diaHorario == null)
                    {
                        return false;
                    }

                    bool esHoy =
                        fechaActual.DayOfWeek == diaHorario.Value;

                    bool yaTermino =
                        fechaActual.TimeOfDay >= h.HoraFin;

                    return esHoy && yaTermino;
                }))
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.IdCurso.ToString(),
                    Text = c.Nombre
                })
                .ToList();
        }

        [Authorize(Roles = "Profesor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProgreso(int id)
        {
            var profesorActual = await _userManager.GetUserAsync(User);

            if (profesorActual == null)
            {
                return Unauthorized();
            }

            var progreso = await _context.ProgresosAlumno
                .FirstOrDefaultAsync(p =>
                    p.IdProgreso == id &&
                    p.IdProfesor == profesorActual.Id);

            if (progreso == null)
            {
                return NotFound();
            }

            _context.ProgresosAlumno.Remove(progreso);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La evaluación fue eliminada correctamente.";

            return RedirectToAction(nameof(ProfesorHistorial));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> VerDetalle(int id)
        {
            var progreso = await _context.ProgresosAlumno
                .Include(p => p.Alumno)
                .Include(p => p.Profesor)
                .Include(p => p.Curso)
                .FirstOrDefaultAsync(p => p.IdProgreso == id);

            if (progreso == null)
            {
                return NotFound();
            }

            return View(progreso);
        }

        private static DayOfWeek? ConvertirDiaSemana(string dia)
        {
            return dia.Trim().ToLower() switch
            {
                "domingo" => DayOfWeek.Sunday,
                "lunes" => DayOfWeek.Monday,
                "martes" => DayOfWeek.Tuesday,
                "miércoles" => DayOfWeek.Wednesday,
                "miercoles" => DayOfWeek.Wednesday,
                "jueves" => DayOfWeek.Thursday,
                "viernes" => DayOfWeek.Friday,
                "sábado" => DayOfWeek.Saturday,
                "sabado" => DayOfWeek.Saturday,
                _ => null
            };
        }


    }
}