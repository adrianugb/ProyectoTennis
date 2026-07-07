using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Helpers;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservasController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Reservar()
        {
            var model = new ReservarCanchaViewModel
            {
                FechaReserva = DateTime.Today
            };

            model.Canchas = await _context.Canchas
                .Where(c => c.Disponible && !c.EnMantenimiento)
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.IdCancha.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(
        ReservarCanchaViewModel model)
        {
            if (!ModelState.IsValid)
                return await CargarVista(model);

            if (model.HoraFin <= model.HoraInicio)
            {
                model.MensajeError =
                    "La hora final debe ser mayor que la hora inicial.";

                return await CargarVista(model);
            }

            if (model.FechaReserva.Date < DateTime.Today)
            {
                model.MensajeError =
                    "No se pueden realizar reservas en fechas anteriores a hoy.";

                return await CargarVista(model);
            }


            var profesor = await _userManager.GetUserAsync(User);

            bool existeChoque = await _context.Reservas.AnyAsync(r =>
                r.IdCancha == model.IdCancha &&
                r.FechaReserva.Date == model.FechaReserva.Date &&
                r.Estado != "Cancelada" &&
                (
                    model.HoraInicio < r.HoraFin &&
                    model.HoraFin > r.HoraInicio
                )
            );

            if (existeChoque)
            {
                model.MensajeError =
                    "La cancha ya está reservada en ese horario.";

                return await CargarVista(model);
            }



            var reserva = new Reserva
            {
                IdCancha = model.IdCancha,
                IdProfesor = profesor.Id,
                FechaReserva = model.FechaReserva,
                HoraInicio = model.HoraInicio,
                HoraFin = model.HoraFin,
                Monto = model.Monto,
                Estado = "Disponible"
            };

            _context.Reservas.Add(reserva);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Reserva creada correctamente.";

            return RedirectToAction(nameof(MisReservas));
        }
        private async Task<IActionResult> CargarVista(
        ReservarCanchaViewModel model)
        {
            model.Canchas = await _context.Canchas
                .Where(c => c.Disponible && !c.EnMantenimiento)
                .Select(c => new SelectListItem
                {
                    Value = c.IdCancha.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();

            return View(model);
        }
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> MisReservas()
        {
            var profesor = await _userManager.GetUserAsync(User);

            var reservas = await _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Alumno)
                .Where(r => r.IdProfesor == profesor.Id)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            var model = new MisReservasProfesorViewModel
            {
                Reservas = reservas.Select(r => new MiReservaProfesorItemViewModel
                {
                    IdReserva = r.IdReserva,
                    Cancha = r.Cancha.Nombre,
                    Fecha = r.FechaReserva,
                    Horario = $"{r.HoraInicio:hh\\:mm} - {r.HoraFin:hh\\:mm}",
                    Estado = r.Estado,
                    Alumno = r.Alumno != null
                        ? $"{r.Alumno.Nombre} {r.Alumno.Apellido}"
                        : "Sin alumno"
                }).ToList()
            };

            return View(model);
        }
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Disponibles()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Profesor)
                .Where(r =>
                    r.IdAlumno == null &&
                    r.Estado == "Disponible")
                .ToListAsync();

            return View(reservas);
        }

        [HttpPost]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ReservarEspacio(int idReserva)
        {
            var alumno = await _userManager.GetUserAsync(User);

            var reserva = await _context.Reservas
                .Include(r => r.Cancha)
                .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

            if (reserva == null)
                return NotFound();

            if (reserva.IdAlumno != null)
            {
                TempData["Error"] = "La reserva ya fue tomada.";
                return RedirectToAction(nameof(Disponibles));
            }

            bool tieneChoque = await _context.Reservas.AnyAsync(r =>
                r.IdAlumno == alumno.Id &&
                r.FechaReserva.Date == reserva.FechaReserva.Date &&
                r.Estado != "Cancelada" &&
                (
                    reserva.HoraInicio < r.HoraFin &&
                    reserva.HoraFin > r.HoraInicio
                )
            );

            if (tieneChoque)
            {
                TempData["Error"] = "Ya tiene una reserva en ese horario.";
                return RedirectToAction(nameof(Disponibles));
            }

            var pago = new Pago
            {
                IdAlumno = alumno.Id,
                Monto = reserva.Monto,
                TipoPago = "Reserva",
                MetodoPago = "Pendiente",
                Estado = "Pendiente",
                FechaPago = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(3),
                EsManual = false,
                IdReserva = reserva.IdReserva,
                Observaciones = "Pago pendiente por reserva de clase especial"
            };

            _context.Pagos.Add(pago);

            var nombreCancha = reserva.Cancha != null
                ? reserva.Cancha.Nombre
                : "la cancha seleccionada";

            await NotificacionHelper.EnviarNotificacionAsync(
            _context,
            alumno.Id,
            categoria: "Clase",
            tipo: "Reserva",
            titulo: "Pago pendiente de reserva",
            mensaje: $"Se generó un pago pendiente por la reserva en {nombreCancha} para el {reserva.FechaReserva:dd/MM/yyyy} de {reserva.HoraInicio:hh\\:mm} a {reserva.HoraFin:hh\\:mm}. Debes pagar para completar la reserva.");

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Se generó el pago pendiente. Debe realizar el pago para completar la reserva.";

            return RedirectToAction("HistorialPagos", "Usuario");
        }

        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> MisReservasAlumno()
        {
            var alumno = await _userManager.GetUserAsync(User);

            var reservas = await _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Profesor)
                .Where(r => r.IdAlumno == alumno.Id)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            return View(reservas);
        }

        [HttpPost]
        [Authorize(Roles = "Usuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiberarReserva(int idReserva)
        {
            var alumno = await _userManager.GetUserAsync(User);

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == idReserva &&
                    r.IdAlumno == alumno.Id);

            if (reserva == null)
                return NotFound();

            var fechaHoraReserva =
                reserva.FechaReserva.Date + reserva.HoraInicio;

            if (fechaHoraReserva <= DateTime.Now.AddHours(24))
            {
                TempData["Error"] =
                    "Solo puede liberar la reserva con al menos 24 horas de anticipación.";

                return RedirectToAction(nameof(MisReservasAlumno));
            }

            reserva.IdAlumno = null;
            reserva.Estado = "Disponible";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Reserva liberada correctamente.";

            return RedirectToAction(nameof(MisReservasAlumno));
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerReserva(int id)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "Reserva no encontrada."
                });
            }

            return Json(new
            {
                exito = true,
                idReserva = reserva.IdReserva,
                fecha = reserva.FechaReserva.ToString("yyyy-MM-dd"),
                horaInicio = reserva.HoraInicio.ToString(@"hh\:mm"),
                horaFin = reserva.HoraFin.ToString(@"hh\:mm")
            });
        }


        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarReserva(
            int idReserva,
            DateTime fechaReserva,
            TimeSpan horaInicio,
            TimeSpan horaFin)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

            if (reserva == null)
            {
                TempData["MensajeError"] =
                    "La reserva no existe.";

                return RedirectToAction(nameof(GestionReservas));
            }

            if (horaFin <= horaInicio)
            {
                TempData["MensajeError"] =
                    "La hora final debe ser mayor que la hora inicial.";

                return RedirectToAction(nameof(GestionReservas));
            }

            bool existeChoque = await _context.Reservas.AnyAsync(r =>
                r.IdReserva != idReserva &&
                r.IdCancha == reserva.IdCancha &&
                r.FechaReserva.Date == fechaReserva.Date &&
                r.Estado != "Cancelada" &&
                (
                    horaInicio < r.HoraFin &&
                    horaFin > r.HoraInicio
                )
            );

            if (existeChoque)
            {
                TempData["MensajeError"] =
                    "Ya existe una reserva para esa cancha en ese horario.";

                return RedirectToAction(nameof(GestionReservas));
            }

            reserva.FechaReserva = fechaReserva;
            reserva.HoraInicio = horaInicio;
            reserva.HoraFin = horaFin;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Reserva actualizada correctamente.";

            return RedirectToAction(nameof(GestionReservas));
        }


        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GestionReservas(DateTime? fecha, int? idCurso, string? estado)
        {
            var reservas = await _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Profesor)
                .Include(r => r.Alumno)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            var matriculasQuery = _context.Matriculas
              .Include(m => m.Alumno)
              .Include(m => m.Curso)
                  .ThenInclude(c => c.Profesor)
              .Include(m => m.Curso)
                  .ThenInclude(c => c.Horarios)
              .AsQueryable();

            if (fecha.HasValue)
            {
                matriculasQuery = matriculasQuery
                    .Where(m => m.FechaMatricula.Date == fecha.Value.Date);
            }

            if (idCurso.HasValue)
            {
                matriculasQuery = matriculasQuery
                    .Where(m => m.IdCurso == idCurso.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                matriculasQuery = matriculasQuery
                    .Where(m => m.Estado == estado);
            }

            var matriculas = await matriculasQuery
                .OrderByDescending(m => m.FechaMatricula)
                .ToListAsync();

            var cursos = await _context.Cursos
                .OrderBy(c => c.Nombre)
                .Select(c => new CursoFiltroViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            var reservasViewModel = reservas.Select(r => new AdminReservaItemViewModel
            {
                IdReserva = r.IdReserva,
                Alumno = r.Alumno != null ? $"{r.Alumno.Nombre} {r.Alumno.Apellido}" : "Sin alumno",
                Curso = "Reserva de cancha",
                Cancha = r.Cancha != null ? r.Cancha.Nombre : "Sin cancha",
                Fecha = r.FechaReserva,
                Horario = $"{r.HoraInicio:hh\\:mm} - {r.HoraFin:hh\\:mm}",
                Profesor = r.Profesor != null ? $"{r.Profesor.Nombre} {r.Profesor.Apellido}" : "Sin profesor",
                Estado = r.Estado
            }).ToList();

            var matriculasViewModel = matriculas.Select(m =>
            {
                var horario = m.Curso?.Horarios?.FirstOrDefault();

                return new AdminReservaItemViewModel
                {
                    IdReserva = m.IdMatricula,
                    Alumno = m.Alumno != null ? $"{m.Alumno.Nombre} {m.Alumno.Apellido}" : "Sin alumno",
                    Curso = m.Curso != null ? m.Curso.Nombre : "Sin curso",
                    Cancha = "No asignada",
                    Fecha = m.FechaMatricula,
                    Horario = horario != null
                        ? $"{horario.DiaSemana} {horario.HoraInicio:hh\\:mm} - {horario.HoraFin:hh\\:mm}"
                        : "Sin horario",
                    Profesor = m.Curso?.Profesor != null
                        ? $"{m.Curso.Profesor.Nombre} {m.Curso.Profesor.Apellidos}"
                        : "Sin profesor",
                    Estado = m.Estado
                };
            }).ToList();

            var model = new AdminReservasViewModel
            {
                MensajeExito = TempData["MensajeExito"]?.ToString(),
                MensajeError = TempData["MensajeError"]?.ToString(),

                Cursos = cursos,

                Reservas = reservasViewModel
               .Concat(matriculasViewModel)
               .OrderByDescending(r => r.Fecha)
               .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarReserva(int idReserva)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

            if (reserva == null)
            {
                TempData["MensajeError"] =
                    "La reserva no existe.";

                return RedirectToAction(nameof(GestionReservas));
            }

            _context.Reservas.Remove(reserva);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Reserva eliminada correctamente.";

            return RedirectToAction(nameof(GestionReservas));
        }

        [HttpGet]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConfirmarPagoReserva(int idReserva)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Profesor)
                .FirstOrDefaultAsync(r =>
                    r.IdReserva == idReserva &&
                    r.IdAlumno == null &&
                    r.Estado == "Disponible");

            if (reserva == null)
            {
                TempData["Error"] = "La reserva no está disponible.";
                return RedirectToAction(nameof(Disponibles));
            }

            var model = new ConfirmarPagoViewModel
            {
                IdReserva = reserva.IdReserva,
                Concepto = $"Reserva en {reserva.Cancha.Nombre} - {reserva.FechaReserva:dd/MM/yyyy}",
                Monto = reserva.Monto
            };

            return View("~/Views/Pagos/ConfirmarPagoReserva.cshtml", model);
        }
    }
}