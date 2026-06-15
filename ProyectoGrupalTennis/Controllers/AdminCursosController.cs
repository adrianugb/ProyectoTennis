using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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


        public AdminCursosController(AppDbContext context)

        {
            _context = context;

        }

        // GET: /AdminCursos/Index
        public async Task<IActionResult> Index(string? buscar)
        {
            var cursos = await _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Profesor)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(buscar))
                cursos = cursos
                    .Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var profesores = await _context.Profesores
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var viewModel = new GestionCursosViewModel
            {
                FiltroBuscar = buscar,
                MensajeExito = TempData["MensajeExito"]?.ToString(),
                MensajeError = TempData["MensajeError"]?.ToString(),
                Profesores = profesores.Select(p => new ProfesorSelectViewModel
                {
                    Id = p.Id.ToString(),
                    NombreCompleto = $"{p.Nombre} {p.Apellidos}"
                }).ToList(),
                Cursos = cursos.Select(c => new CursoAdminItemViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre,
                    Nivel = c.Nivel,
                    Activo = c.Activo,
                    NombreProfesor = c.Profesor != null
                        ? $"{c.Profesor.Nombre} {c.Profesor.Apellidos}"
                        : null,
                    Horarios = c.Horarios != null
                        ? c.Horarios.Select(h =>
                            $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
                        : new List<string>()
                }).ToList()
            };

            return View("~/Views/Perfiles/AdminCursos.cshtml", viewModel);
        }

        // GET: /AdminCursos/ObtenerProfesor/5
        [HttpGet]
        public async Task<IActionResult> ObtenerProfesor(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Profesor)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
                return Json(new { exito = false, mensaje = "El curso no existe." });

            return Json(new
            {
                exito = true,
                idCurso = curso.IdCurso,
                nombreCurso = curso.Nombre,
                idProfesorActual = curso.IdProfesor ?? 0
            });
        }

        // POST: /AdminCursos/AsignarProfesor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarProfesor(int idCurso, int idProfesor)
        {
            var curso = await _context.Cursos.FindAsync(idCurso);
            if (curso == null)
            {
                TempData["MensajeError"] = "El curso no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            var profesor = await _context.Profesores.FindAsync(idProfesor);
            if (profesor == null || !profesor.Activo)
            {
                TempData["MensajeError"] = "El profesor seleccionado no es válido.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar conflicto de horario
            var horariosNuevoCurso = await _context.Horarios
                .Where(h => h.IdCurso == idCurso)
                .ToListAsync();

            var cursosProfesor = await _context.Cursos
                .Include(c => c.Horarios)
                .Where(c => c.IdProfesor == idProfesor && c.IdCurso != idCurso)
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

            curso.IdProfesor = idProfesor;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                $"Profesor '{profesor.Nombre} {profesor.Apellidos}' asignado al curso '{curso.Nombre}' correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminCursos/QuitarProfesor
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

            curso.IdProfesor = null;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Profesor removido del curso '{curso.Nombre}'.";
            return RedirectToAction(nameof(Index));
        }
        //agregar curso
        [HttpPost]
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

        public IActionResult Editar(int id)
        {
            var curso = _service.ObtenerPorId(id);
            if (curso == null) return NotFound();
            var vm = new CursoFormViewModel
            {
                Curso = curso,
                Profesores = _service.ObtenerProfesores()
            };
            return View("~/Views/Cursos/Editar.cshtml", vm);
        }

        [HttpPost]
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
        public IActionResult CambiarEstado(int id, bool activo)
        {
            _service.CambiarEstado(id, activo);
            return RedirectToAction(nameof(Index));
        }
    }

}