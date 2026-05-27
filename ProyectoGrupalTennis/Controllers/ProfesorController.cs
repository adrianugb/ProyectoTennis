using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoGrupalTennis.Controllers
{
    public class ProfesorController : Controller
    {
        private readonly IProfesorService _service;

        public ProfesorController(IProfesorService service)
        {
            _service = service;
        }

        // Listar
        public IActionResult Index(string? buscar, string? especialidad, string? estado)
        {
            var profesores = _service.ObtenerTodos();

            if (!string.IsNullOrEmpty(buscar))
                profesores = profesores
                    .Where(p => p.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                             || p.Apellidos.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(especialidad))
                profesores = profesores
                    .Where(p => p.Especialidad == especialidad).ToList();

            if (estado == "Activo")
                profesores = profesores.Where(p => p.Activo).ToList();
            else if (estado == "Inactivo")
                profesores = profesores.Where(p => !p.Activo).ToList();

            return View(profesores);
        }

        // Agregar - GET
        public IActionResult Agregar() => View(new Profesor());

        // Agregar - POST
        [HttpPost]
        public IActionResult Agregar(Profesor profesor)
        {
            if (!ModelState.IsValid) return View(profesor);
            _service.Agregar(profesor);
            return RedirectToAction(nameof(Index));
        }

        // Editar - GET
        public IActionResult Editar(int id)
        {
            var profesor = _service.ObtenerPorId(id);
            if (profesor == null) return NotFound();
            return View(profesor);
        }

        // Editar - POST
        [HttpPost]
        public IActionResult Editar(Profesor profesor)
        {
            if (!ModelState.IsValid) return View(profesor);
            _service.Actualizar(profesor);
            return RedirectToAction(nameof(Index));
        }

        // Activar/Desactivar
        [HttpPost]
        public IActionResult CambiarEstado(int id, bool activo)
        {
            _service.CambiarEstado(id, activo);
            return RedirectToAction(nameof(Index));
        }
    }
}