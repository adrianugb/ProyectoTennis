using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminCursosController : Controller
    {
        private readonly ICursoService _service;
        private readonly AppDbContext _context;

        public AdminCursosController(AppDbContext context, ICursoService service)
        {
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> Index(string? buscar)
        {
            var cursos = await _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Profesor)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                cursos = cursos
                    .Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var profesores = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                where r.Name == "Profesor"
                orderby u.Nombre
                select u
            ).ToListAsync();

            var viewModel = new GestionCursosViewModel
            {
                FiltroBuscar = buscar,
                MensajeExito = TempData["MensajeExito"]?.ToString(),
                MensajeError = TempData["MensajeError"]?.ToString(),

                Profesores = profesores.Select(p => new ProfesorSelectViewModel
                {
                    Id = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellido}"
                }).ToList(),

                Cursos = cursos.Select(c => new CursoAdminItemViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre,
                    Nivel = c.Nivel,
                    Activo = c.Activo,
                    NombreProfesor = c.Profesor != null
                        ? $"{c.Profesor.Nombre} {c.Profesor.Apellido}"
                        : null,
                    Horarios = c.Horarios != null
                        ? c.Horarios.Select(h =>
                            $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
                        : new List<string>()
                }).ToList()
            };

            return View("~/Views/Perfiles/AdminCursos.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProfesor(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Profesor)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
            {
                return Json(new { exito = false, mensaje = "El curso no existe." });
            }

            return Json(new
            {
                exito = true,
                idCurso = curso.IdCurso,
                nombreCurso = curso.Nombre,
                idProfesorActual = curso.IdProfesorUserId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarProfesor(int idCurso, string idProfesorUserId)
        {
            var curso = await _context.Cursos.FindAsync(idCurso);

            if (curso == null)
            {
                TempData["MensajeError"] = "El curso no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            var profesor = await _context.Users.FirstOrDefaultAsync(u => u.Id == idProfesorUserId);

            if (profesor == null)
            {
                TempData["MensajeError"] = "El profesor seleccionado no es válido.";
                return RedirectToAction(nameof(Index));
            }

            var horariosNuevoCurso = await _context.Horarios
                .Where(h => h.IdCurso == idCurso)
                .ToListAsync();

            var cursosProfesor = await _context.Cursos
                .Include(c => c.Horarios)
                .Where(c => c.IdProfesorUserId == idProfesorUserId && c.IdCurso != idCurso)
                .ToListAsync();

            foreach (var cursoExistente in cursosProfesor)
            {
                foreach (var horarioExistente in cursoExistente.Horarios)
                {
                    foreach (var horarioNuevo in horariosNuevoCurso)
                    {
                        if (horarioExistente.DiaSemana == horarioNuevo.DiaSemana &&
                            horarioExistente.HoraInicio < horarioNuevo.HoraFin &&
                            horarioExistente.HoraFin > horarioNuevo.HoraInicio)
                        {
                            TempData["MensajeError"] =
                                $"Conflicto de horario con el curso '{cursoExistente.Nombre}' " +
                                $"el {horarioExistente.DiaSemana} de " +
                                $"{horarioExistente.HoraInicio:hh\\:mm} a {horarioExistente.HoraFin:hh\\:mm}.";

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }

            curso.IdProfesorUserId = idProfesorUserId;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                $"Profesor '{profesor.Nombre} {profesor.Apellido}' asignado al curso '{curso.Nombre}' correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarProfesor(int idCurso)
        {
            var curso = await _context.Cursos.FindAsync(idCurso);

            if (curso == null)
            {
                TempData["MensajeError"] = "El curso no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            curso.IdProfesorUserId = null;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Profesor removido del curso '{curso.Nombre}'.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Agregar()
        {
            var vm = new CursoFormViewModel
            {
                Curso = new Curso(),
                Profesores = _service.ObtenerProfesores()
            };

            return View("~/Views/Cursos/Agregar.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(CursoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Agregar.cshtml", vm);
            }

            _service.Agregar(vm.Curso);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var curso = _service.ObtenerPorId(id);

            if (curso == null)
            {
                return NotFound();
            }

            var vm = new CursoFormViewModel
            {
                Curso = curso,
                Profesores = _service.ObtenerProfesores()
            };

            return View("~/Views/Cursos/Editar.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(CursoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Editar.cshtml", vm);
            }

            _service.Actualizar(vm.Curso);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id, bool activo)
        {
            _service.CambiarEstado(id, activo);
            return RedirectToAction(nameof(Index));
        }
    }
}