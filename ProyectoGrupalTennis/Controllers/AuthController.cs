using AcademiaTennisDAL.Entities;
using AcademiaTennisDAL.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    public class AuthController : Controller
    {
        // UserManager permite gestionar usuarios de Identity:
        // crear usuarios, buscar correos, validar información, etc.
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.Email = model.Email.Trim().ToLower();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Busca el usuario por correo
            var usuario = await _userManager.FindByEmailAsync(model.Email);

            // Verifica si el usuario está bloqueado
            if (usuario != null && usuario.Bloqueado)
            {
                ModelState.AddModelError("", "Tu cuenta ha sido bloqueada.");
                return View(model);
            }

            // Intenta iniciar sesión
            var resultado = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.Recordarme,
                lockoutOnFailure: false
            );

            // Login exitoso
            if (resultado.Succeeded)
            {
                return RedirectToAction("PerfilUsuario", "Home");
            }

            // Credenciales incorrectas
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Registro(RegisterViewModel model)
        {
            model.Email = model.Email.Trim().ToLower();  // Elimina espacios vacíos y convierte el correo a minúsculas
            model.Nombre = model.Nombre.Trim();     // Elimina espacios al inicio o final del nombre y apellido
            model.Apellido = model.Apellido.Trim();

            // Verifica si las validaciones del modelo son correctas.
            // Ejemplo:
            // - campos obligatorios
            // - contraseña mínima
            // - confirmación de contraseña
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Busca si ya existe un usuario con ese correo
            var usuarioExistente = await _userManager.FindByEmailAsync(model.Email);

            // Si el correo ya existe, muestra mensaje de error
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("Email", "El correo ya está registrado.");
                return View(model);
            }

            // Crea un nuevo objeto ApplicationUser
            // con la información ingresada en el formulario
            var usuario = new ApplicationUser
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                UserName = model.Email,
                Email = model.Email,
                FechaRegistro = DateTime.Now,
                Bloqueado = false
            };

            // Crea el usuario en AspNetUsers
            // y guarda la contraseña en formato hash
            var resultado = await _userManager.CreateAsync(usuario, model.Password);

            if (resultado.Succeeded)
            {
                var rolResultado = await _userManager.AddToRoleAsync(usuario, "Usuario");
                if (!rolResultado.Succeeded)
                {
                    await _userManager.DeleteAsync(usuario);
                    ModelState.AddModelError("", "Error al asignar el rol. Intente nuevamente.");
                    return View(model);
                }

                await _signInManager.SignInAsync(usuario, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Si hubo errores de Identity  los agrega al formulario para mostrarlos en pantalla
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }



    }
}