using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;
using System.Security.Claims;
using AcademiaTennisDAL.Context;

namespace ProyectoGrupalTennis.Controllers
{
    public class CursoController : Controller
    {
        private readonly ICursoService _service;
        private readonly AppDbContext _context;

        public CursoController(
            ICursoService service,
            AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        public IActionResult Index(string? buscar, string? nivel, string? estado)
        {
            var cursos = _service.ObtenerTodos();

            if (!string.IsNullOrEmpty(buscar))
                cursos = cursos
                    .Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(nivel))
                cursos = cursos.Where(c => c.Nivel == nivel).ToList();

            if (estado == "Activo")
                cursos = cursos.Where(c => c.Activo).ToList();
            else if (estado == "Inactivo")
                cursos = cursos.Where(c => !c.Activo).ToList();

            return View("~/Views/Cursos/Index.cshtml", cursos);
        }

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

            if (vm.NuevoHorario != null)
            {
                vm.NuevoHorario.IdCurso = vm.Curso.IdCurso;
                _service.AgregarHorario(vm.NuevoHorario);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var curso = _service.ObtenerPorId(id); // ahora trae Profesor y Horarios
            if (curso == null) return NotFound();
            var vm = new CursoFormViewModel
            {
                Curso = curso,
                Profesores = _service.ObtenerProfesores(),
                Horarios = _service.ObtenerHorarios(id)
            };
            return View("~/Views/Cursos/Editar.cshtml", vm);
        }

        [HttpPost]
        public IActionResult AgregarHorario(CursoFormViewModel vm)
        {
            vm.NuevoHorario.IdCurso = vm.Curso.IdCurso;
            try { _service.AgregarHorario(vm.NuevoHorario); }
            catch (Exception ex) { TempData["MensajeError"] = ex.Message; }
            return RedirectToAction(nameof(Editar), new { id = vm.Curso.IdCurso });
        }

        [HttpPost]
        public IActionResult EliminarHorario(int idHorario, int idCurso)
        {
            _service.EliminarHorario(idHorario);
            return RedirectToAction(nameof(Editar), new { id = idCurso });
        }

        [HttpPost]
        public IActionResult CambiarEstado(int id, bool activo)
        {
            _service.CambiarEstado(id, activo);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ReservarCurso(int idCurso)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Account");

            var curso = _context.Cursos.Find(idCurso);

            if (curso == null)
                return NotFound();

            if (curso.CuposDisponibles <= 0)
            {
                TempData["Error"] = "No hay cupos disponibles.";
                return RedirectToAction(nameof(Index));
            }

            bool yaMatriculado = _context.Matriculas.Any(m =>
                m.IdCurso == idCurso &&
                m.IdAlumno == usuarioId &&
                m.Estado == "Activa");

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado en este curso.";
                return RedirectToAction(nameof(Index));
            }

            _context.Matriculas.Add(new Matricula
            {
                IdAlumno = usuarioId,
                IdCurso = idCurso,
                FechaMatricula = DateTime.Now,
                Estado = "Activa"
            });

            curso.CuposDisponibles--;

            _context.SaveChanges();

            TempData["Exito"] = "Curso reservado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
