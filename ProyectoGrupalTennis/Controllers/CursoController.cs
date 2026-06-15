using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    public class CursoController : Controller
    {
        private readonly ICursoService _service;

        public CursoController(ICursoService service)
        {
            _service = service;
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
    }
}
