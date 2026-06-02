using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Agregar() =>
            View("~/Views/Cursos/Agregar.cshtml", new Curso());

        [HttpPost]
        public IActionResult Agregar(Curso curso)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Cursos/Agregar.cshtml", curso);
            _service.Agregar(curso);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var curso = _service.ObtenerPorId(id);
            if (curso == null) return NotFound();
            return View("~/Views/Cursos/Editar.cshtml", curso);
        }

        [HttpPost]
        public IActionResult Editar(Curso curso)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Cursos/Editar.cshtml", curso);
            _service.Actualizar(curso);
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