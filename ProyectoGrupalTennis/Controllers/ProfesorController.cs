using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    public class ProfesorController : Controller
    {
        private readonly IProfesorService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfesorController(IProfesorService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public IActionResult Index(string? buscar, string? especialidad, string? estado)
        {
            var profesores = _service.ObtenerTodos();

            if (!string.IsNullOrEmpty(buscar))
                profesores = profesores
                    .Where(p => p.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                             || p.Apellidos.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(especialidad))
                profesores = profesores.Where(p => p.Especialidad == especialidad).ToList();

            if (estado == "Activo")
                profesores = profesores.Where(p => p.Activo).ToList();
            else if (estado == "Inactivo")
                profesores = profesores.Where(p => !p.Activo).ToList();

            return View(profesores);
        }

        public async Task<IActionResult> Agregar()
        {
            var usuarios = await _userManager.GetUsersInRoleAsync("Profesor");
            ViewBag.Usuarios = usuarios.Select(u => new { u.Id, Nombre = $"{u.Nombre} {u.Apellido} ({u.Email})" }).ToList();
            return View(new Profesor());
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(Profesor profesor)
        {
            if (!ModelState.IsValid)
            {
                var usuarios = await _userManager.GetUsersInRoleAsync("Profesor");
                ViewBag.Usuarios = usuarios.Select(u => new { u.Id, Nombre = $"{u.Nombre} {u.Apellido} ({u.Email})" }).ToList();
                return View(profesor);
            }
            _service.Agregar(profesor);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var profesor = _service.ObtenerPorId(id);
            if (profesor == null) return NotFound();
            var usuarios = await _userManager.GetUsersInRoleAsync("Profesor");
            ViewBag.Usuarios = usuarios.Select(u => new { u.Id, Nombre = $"{u.Nombre} {u.Apellido} ({u.Email})" }).ToList();
            return View(profesor);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Profesor profesor)
        {
            if (!ModelState.IsValid)
            {
                var usuarios = await _userManager.GetUsersInRoleAsync("Profesor");
                ViewBag.Usuarios = usuarios.Select(u => new { u.Id, Nombre = $"{u.Nombre} {u.Apellido} ({u.Email})" }).ToList();
                return View(profesor);
            }
            _service.Actualizar(profesor);
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