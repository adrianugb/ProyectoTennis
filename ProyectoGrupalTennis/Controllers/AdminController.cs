
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models.ViewModels;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();

            var modelo = new List<UsuarioRolViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);

                modelo.Add(new UsuarioRolViewModel
                {
                    Usuario = usuario,
                    Rol = roles.FirstOrDefault() ?? "Sin Rol"
                });
            }

            return View(modelo);
        }

        // HACER PROFESOR
        public async Task<IActionResult> HacerProfesor(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Verifica si ya es profesor
            if (!await _userManager.IsInRoleAsync(usuario, "Profesor"))
            {
                // Quitar rol Usuario
                await _userManager.RemoveFromRoleAsync(usuario, "Usuario");

                // Agregar rol Profesor
                await _userManager.AddToRoleAsync(usuario, "Profesor");
            }

            return RedirectToAction("Index");
        }

        // HACER USUARIO NORMAL
        public async Task<IActionResult> HacerUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Quitar rol Profesor
            await _userManager.RemoveFromRoleAsync(usuario, "Profesor");

            // Agregar rol Usuario
            await _userManager.AddToRoleAsync(usuario, "Usuario");

            return RedirectToAction("Index");
        }

        // BLOQUEAR USUARIO
        public async Task<IActionResult> BloquearUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Bloqueado = true;

            await _userManager.UpdateAsync(usuario);

            return RedirectToAction("Index");
        }

        // DESBLOQUEAR USUARIO
        public async Task<IActionResult> DesbloquearUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Bloqueado = false;

            await _userManager.UpdateAsync(usuario);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CambiarRol(string id, string nuevoRol)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // proteger admin principal
            if (usuario.Email == "admin@tennis.com")
            {
                return RedirectToAction("Index");
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);

            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);

            await _userManager.AddToRoleAsync(usuario, nuevoRol);

            return RedirectToAction("Index");
        }

    }
}
