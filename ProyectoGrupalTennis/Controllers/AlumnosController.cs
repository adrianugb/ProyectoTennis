using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AlumnosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AlumnosController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Alumnos/Index
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            var alumnos = await _userManager.GetUsersInRoleAsync("Usuario");

            if (!string.IsNullOrEmpty(buscar))
                alumnos = alumnos
                    .Where(a => a.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                             || a.Apellido.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                             || (a.Email ?? "").Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (estado == "Activo")
                alumnos = alumnos.Where(a => !a.Bloqueado).ToList();
            else if (estado == "Inactivo")
                alumnos = alumnos.Where(a => a.Bloqueado).ToList();

            return View("~/Views/Alumnos/Index.cshtml", alumnos.OrderBy(a => a.Nombre).ToList());
        }

        // GET: /Alumnos/Agregar
        public IActionResult Agregar() => View("~/Views/Alumnos/Agregar.cshtml");

        // POST: /Alumnos/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(string Nombre, string Apellido,
            string Email, string Telefono, string Contrasena)
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido)
                || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Contrasena))
            {
                ViewBag.Error = "Todos los campos obligatorios deben completarse.";
                return View("~/Views/Alumnos/Agregar.cshtml");
            }

            var correo = Email.Trim().ToLower();
            var existente = await _userManager.FindByEmailAsync(correo);
            if (existente != null)
            {
                ViewBag.Error = "Ya existe un alumno con ese correo electrónico.";
                return View("~/Views/Alumnos/Agregar.cshtml");
            }

            var alumno = new ApplicationUser
            {
                Nombre = Nombre.Trim(),
                Apellido = Apellido.Trim(),
                UserName = correo,
                Email = correo,
                PhoneNumber = Telefono?.Trim(),
                FechaRegistro = DateTime.Now,
                Bloqueado = false
            };

            var resultado = await _userManager.CreateAsync(alumno, Contrasena);
            if (!resultado.Succeeded)
            {
                ViewBag.Error = string.Join(" ", resultado.Errors.Select(e => e.Description));
                return View("~/Views/Alumnos/Agregar.cshtml");
            }

            await _userManager.AddToRoleAsync(alumno, "Usuario");
            return RedirectToAction(nameof(Index));
        }

        // GET: /Alumnos/Editar/id
        public async Task<IActionResult> Editar(string id)
        {
            var alumno = await _userManager.FindByIdAsync(id);
            if (alumno == null) return NotFound();
            return View("~/Views/Alumnos/Editar.cshtml", alumno);
        }

        // POST: /Alumnos/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(string Id, string Nombre, string Apellido,
            string Email, string Telefono)
        {
            var alumno = await _userManager.FindByIdAsync(Id);
            if (alumno == null) return NotFound();

            var correo = Email.Trim().ToLower();
            if (!string.Equals(alumno.Email, correo, StringComparison.OrdinalIgnoreCase))
            {
                var enUso = await _userManager.FindByEmailAsync(correo);
                if (enUso != null && enUso.Id != alumno.Id)
                {
                    ViewBag.Error = "El correo ingresado ya está en uso.";
                    return View("~/Views/Alumnos/Editar.cshtml", alumno);
                }
                alumno.Email = correo;
                alumno.UserName = correo;
            }

            alumno.Nombre = Nombre.Trim();
            alumno.Apellido = Apellido.Trim();
            alumno.PhoneNumber = Telefono?.Trim();

            var resultado = await _userManager.UpdateAsync(alumno);
            if (!resultado.Succeeded)
            {
                ViewBag.Error = string.Join(" ", resultado.Errors.Select(e => e.Description));
                return View("~/Views/Alumnos/Editar.cshtml", alumno);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Alumnos/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(string id, bool activo)
        {
            var alumno = await _userManager.FindByIdAsync(id);
            if (alumno != null)
            {
                alumno.Bloqueado = !activo;
                await _userManager.UpdateAsync(alumno);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}