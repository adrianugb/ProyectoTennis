using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    // Solo administradores pueden acceder a este controlador
    [Authorize(Roles = "Administrador")]
    public class AlumnosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AlumnosController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        // GET: /Alumnos/Index
        // Muestra la lista de alumnos con rol "Usuario"

        public async Task<IActionResult> Index(string? mensajeExito = null, string? mensajeError = null)
        {
            var alumnos = await _userManager.GetUsersInRoleAsync("Usuario");

            var viewModel = new AdminAlumnosViewModel
            {
                Alumnos = alumnos
                    .OrderBy(a => a.Nombre)
                    .Select(a => new AlumnoListItemViewModel
                    {
                        Id = a.Id,
                        NombreCompleto = $"{a.Nombre} {a.Apellido}",
                        Correo = a.Email ?? string.Empty,
                        Telefono = a.PhoneNumber ?? "No registrado",
                        Activo = !a.Bloqueado,
                        FechaRegistro = a.FechaRegistro
                    })
                    .ToList(),

                MensajeExito = mensajeExito,
                MensajeError = mensajeError
            };

            return View("~/Views/Perfiles/AdminAlumnos.cshtml", viewModel);
        }

        // Alumnos/Crear  →  Historia 1: Agregar alumno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearAlumnoViewModel model)
        {
            // 1. Validar campos obligatorios del modelo
            if (!ModelState.IsValid)
            {
                return await RecargarConErrores(model, null, "Por favor, corrija los errores del formulario.");
            }

            var correoNormalizado = model.Correo.Trim().ToLower();

            // 2. Validar que no exista un alumno con el mismo correo (evitar duplicados)
            var usuarioExistente = await _userManager.FindByEmailAsync(correoNormalizado);
            if (usuarioExistente != null)
            {
                return await RecargarConErrores(model, null, "Ya existe un alumno registrado con ese correo electrónico.");
            }

            // 3. Crear el nuevo usuario
            var nuevoAlumno = new ApplicationUser
            {
                Nombre = model.Nombre.Trim(),
                Apellido = model.Apellido.Trim(),
                UserName = correoNormalizado,
                Email = correoNormalizado,
                PhoneNumber = model.Telefono.Trim(),
                FechaRegistro = DateTime.Now,
                Bloqueado = false
            };

            var resultado = await _userManager.CreateAsync(nuevoAlumno, model.Contrasena);

            if (!resultado.Succeeded)
            {
                var errores = string.Join(" ", resultado.Errors.Select(e => e.Description));
                return await RecargarConErrores(model, null, errores);
            }

            // 4. Asignar rol "Usuario" al nuevo alumno
            await _userManager.AddToRoleAsync(nuevoAlumno, "Usuario");

            return RedirectToAction(nameof(Index),
                new { mensajeExito = $"El alumno {nuevoAlumno.Nombre} {nuevoAlumno.Apellido} fue registrado exitosamente." });
        }

        // GET: /Alumnos/ObtenerDatos/{id}  →  Historia 2: Cargar datos para editar
        // Retorna JSON para llenar el modal de edición desde el cliente

        [HttpGet]
        public async Task<IActionResult> ObtenerDatos(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { exito = false, mensaje = "ID de alumno no válido." });

            var alumno = await _userManager.FindByIdAsync(id);

            if (alumno == null)
                return Json(new { exito = false, mensaje = "El alumno solicitado no existe en el sistema." });

            return Json(new
            {
                exito = true,
                id = alumno.Id,
                nombre = alumno.Nombre,
                apellido = alumno.Apellido,
                correo = alumno.Email,
                telefono = alumno.PhoneNumber
            });
        }


        // GET: /Alumnos/Editar  →  Historia 2: Modificar alumno

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarAlumnoViewModel model)
        {
            // 1. Validar campos obligatorios
            if (!ModelState.IsValid)
            {
                var errores = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return await RecargarConErrores(null, model, errores);
            }

            // 2. Verificar que el alumno exista
            var alumno = await _userManager.FindByIdAsync(model.Id);
            if (alumno == null)
            {
                return await RecargarConErrores(null, model, "El alumno que intenta modificar no existe en el sistema.");
            }

            var correoNormalizado = model.Correo.Trim().ToLower();

            // 3. Si cambió el correo, verificar que el nuevo no esté en uso por otro usuario
            if (!string.Equals(alumno.Email, correoNormalizado, StringComparison.OrdinalIgnoreCase))
            {
                var correoEnUso = await _userManager.FindByEmailAsync(correoNormalizado);
                if (correoEnUso != null && correoEnUso.Id != alumno.Id)
                {
                    return await RecargarConErrores(null, model, "El correo ingresado ya está en uso por otro alumno.");
                }

                // Actualizar correo y username
                alumno.Email = correoNormalizado;
                alumno.UserName = correoNormalizado;
            }

            // 4. Actualizar los campos editables
            alumno.Nombre = model.Nombre.Trim();
            alumno.Apellido = model.Apellido.Trim();
            alumno.PhoneNumber = model.Telefono.Trim();

            var resultado = await _userManager.UpdateAsync(alumno);

            if (!resultado.Succeeded)
            {
                var errores = string.Join(" ", resultado.Errors.Select(e => e.Description));
                return await RecargarConErrores(null, model, errores);
            }

            return RedirectToAction(nameof(Index),
                new { mensajeExito = $"La información de {alumno.Nombre} {alumno.Apellido} fue actualizada exitosamente." });
        }


        // POST: /Alumnos/Desactivar/{id}  →  Historia 3: Desactivar alumno (bloquear)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(Index), new { mensajeError = "ID de alumno no válido." });

            // 1. Verificar que el alumno exista
            var alumno = await _userManager.FindByIdAsync(id);
            if (alumno == null)
            {
                return RedirectToAction(nameof(Index),
                    new { mensajeError = "El alumno que intenta desactivar no existe en el sistema." });
            }

            // 2. Verificar que el alumno no esté ya inactivo
            if (alumno.Bloqueado)
            {
                return RedirectToAction(nameof(Index),
                    new { mensajeError = $"El alumno {alumno.Nombre} {alumno.Apellido} ya se encuentra inactivo." });
            }

            // 3. Desactivar el alumno marcándolo como bloqueado
            alumno.Bloqueado = true;
            var resultado = await _userManager.UpdateAsync(alumno);

            if (!resultado.Succeeded)
            {
                var errores = string.Join(" ", resultado.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index), new { mensajeError = errores });
            }

            return RedirectToAction(nameof(Index),
                new { mensajeExito = $"El alumno {alumno.Nombre} {alumno.Apellido} fue desactivado exitosamente." });
        }


        // Método privado: recarga la vista con mensajes de error sin perder
        // el estado del formulario (evita que el admin pierda lo que escribió)

        private async Task<IActionResult> RecargarConErrores(
            CrearAlumnoViewModel? modelCrear,
            EditarAlumnoViewModel? modelEditar,
            string mensajeError)
        {
            var alumnos = await _userManager.GetUsersInRoleAsync("Usuario");

            var viewModel = new AdminAlumnosViewModel
            {
                Alumnos = alumnos
                    .OrderBy(a => a.Nombre)
                    .Select(a => new AlumnoListItemViewModel
                    {
                        Id = a.Id,
                        NombreCompleto = $"{a.Nombre} {a.Apellido}",
                        Correo = a.Email ?? string.Empty,
                        Telefono = a.PhoneNumber ?? "No registrado",
                        Activo = !a.Bloqueado,
                        FechaRegistro = a.FechaRegistro
                    })
                    .ToList(),

                NuevoAlumno = modelCrear ?? new CrearAlumnoViewModel(),
                AlumnoAEditar = modelEditar,
                MensajeError = mensajeError
            };

            return View("~/Views/Perfiles/AdminAlumnos.cshtml", viewModel);
        }
    }
}