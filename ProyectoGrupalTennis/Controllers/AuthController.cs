using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;

namespace ProyectoGrupalTennis.Controllers
{
    public class AuthController : Controller
    {
        #region Dependencias

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        #endregion

        #region Registro

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegisterViewModel model)
        {
            model.Email = model.Email.Trim().ToLower();
            model.Nombre = model.Nombre.Trim();
            model.Apellido = model.Apellido.Trim();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuarioExistente = await _userManager.FindByEmailAsync(model.Email);

            if (usuarioExistente != null)
            {
                ModelState.AddModelError("Email", "El correo ya está registrado.");
                return View(model);
            }

            var usuario = new ApplicationUser
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                UserName = model.Email,
                Email = model.Email,
                FechaRegistro = DateTime.Now,
                Bloqueado = false
            };

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

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        #endregion

        #region Login

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.Email = model.Email.Trim().ToLower();

            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _userManager.FindByEmailAsync(model.Email);

            if (usuario != null && usuario.Bloqueado)
            {
                ModelState.AddModelError("", "Tu cuenta ha sido bloqueada.");
                return View(model);
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.Recordarme,
                lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                // Redirigir según el rol del usuario
                if (await _userManager.IsInRoleAsync(usuario!, "Administrador"))
                    return RedirectToAction("PerfilAdmin", "Home");

                if (await _userManager.IsInRoleAsync(usuario!, "Profesor"))
                    return RedirectToAction("PerfilProfesor", "Home");

                return RedirectToAction("PerfilUsuario", "Home");
            }

            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(model);
        }

        #endregion

        #region Logout

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Recuperar contraseña

        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarContrasena(RecuperarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim().ToLower();
            var usuario = await _userManager.FindByEmailAsync(email);

            if (usuario == null)
            {
                ModelState.AddModelError("", "No existe ninguna cuenta registrada con ese correo.");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            var link = Url.Action(
                "RestablecerPassword",
                "Auth",
                new
                {
                    email = usuario.Email,
                    token = token
                },
                Request.Scheme);

            var asunto = "Restablecer contraseña - Academia de Tennis";

            var mensaje = $@"
    <h2>Restablecer contraseña</h2>
    <p>Hola,</p>
    <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
    <p>Haz clic en el siguiente enlace para crear una nueva contraseña:</p>
    <p><a href='{link}'>Restablecer contraseña</a></p>
    <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
";

            await _emailService.EnviarCorreoAsync(usuario.Email, asunto, mensaje);

            ViewBag.Mensaje = "Se han enviado instrucciones para restablecer la contraseña a tu correo.";

            return View();
        }

        [HttpGet]
        public IActionResult RestablecerPassword(string token, string email)
        {
            var model = new RestablecerPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _userManager.FindByEmailAsync(model.Email);

            if (usuario == null)
            {
                ModelState.AddModelError("", "No se encontró el usuario.");
                return View(model);
            }

            var resultado = await _userManager.ResetPasswordAsync(
                usuario,
                model.Token,
                model.NuevaPassword);

            if (resultado.Succeeded)
            {
                TempData["Mensaje"] = "Contraseña actualizada correctamente.";
                return RedirectToAction("Login");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        #endregion



        #region Cambiar contraseña

        [Authorize]
        [HttpGet]
        public IActionResult CambiarPassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Debe completar correctamente todos los campos."
                });
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Debe iniciar sesión nuevamente."
                });
            }

            var resultado = await _userManager.ChangePasswordAsync(
                usuario,
                model.PasswordActual,
                model.NuevaPassword);

            if (resultado.Succeeded)
            {
                return Json(new
                {
                    success = true,
                    message = "Contraseña actualizada correctamente."
                });
            }

            foreach (var error in resultado.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    return Json(new
                    {
                        success = false,
                        message = "La contraseña actual es incorrecta."
                    });
                }
            }

            return Json(new
            {
                success = false,
                message = "No se pudo cambiar la contraseña. Verifique los datos ingresados."
            });
        }

        #endregion

        #region Acceso denegado

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion
    }
}