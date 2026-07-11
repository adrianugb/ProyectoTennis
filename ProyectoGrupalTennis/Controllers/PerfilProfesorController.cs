using AcademiaTennisDAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using AcademiaTennisDAL.Entities;
using ProyectoGrupalTennis.Helpers;
using ProyectoGrupalTennis.Services;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Profesor")]
    public class PerfilProfesorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public PerfilProfesorController(AppDbContext context, UserManager<ApplicationUser> userManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        private async Task<Profesor?> ObtenerProfesorActual()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Profesores
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        // GET: /PerfilProfesor/MisCursos
        public async Task<IActionResult> MisCursos(string? buscar, string? nivel)
        {
            var profesor = await ObtenerProfesorActual();

            var viewModel = new ProfesorMisCursosViewModel
            {
                FiltroBuscar = buscar,
                FiltroNivel = nivel,
                MensajeExito = TempData["MensajeExito"]?.ToString(),
                MensajeError = TempData["MensajeError"]?.ToString(),
                Cursos = new List<CursoProfesorItemViewModel>()
            };

            if (profesor == null)
            {
                viewModel.MensajeError = "Tu cuenta no tiene un perfil de profesor vinculado. Contactá al administrador.";
                return View("~/Views/Perfiles/ProfesorCursos.cshtml", viewModel);
            }

            var query = _context.Cursos
                .Include(c => c.Horarios)
                .Where(c => c.IdProfesor == profesor.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(c => c.Nombre.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(nivel))
                query = query.Where(c => c.Nivel == nivel);

            var cursos = await query.OrderBy(c => c.Nombre).ToListAsync();

            viewModel.Cursos = cursos.Select(c => new CursoProfesorItemViewModel
            {
                IdCurso = c.IdCurso,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion ?? string.Empty,
                Nivel = c.Nivel,
                CuposDisponibles = c.CuposDisponibles,
                Activo = c.Activo,
                Horarios = c.Horarios != null
                    ? c.Horarios.Select(h => new HorarioProfesorItemViewModel
                    {
                        IdHorario = h.IdHorario,
                        DiaSemana = h.DiaSemana,
                        Fecha = h.Fecha.ToString("yyyy-MM-dd"),
                        HoraInicio = $"{h.HoraInicio.Hours:D2}:{h.HoraInicio.Minutes:D2}",
                        HoraFin = $"{h.HoraFin.Hours:D2}:{h.HoraFin.Minutes:D2}"
                    }).ToList()
                    : new List<HorarioProfesorItemViewModel>()
            }).ToList();

            return View("~/Views/Perfiles/ProfesorCursos.cshtml", viewModel);
        }

        // GET: /PerfilProfesor/ReprogramarClase/5
        public async Task<IActionResult> ReprogramarClase(int idHorario)
        {
            var horario = await _context.Horarios
                .Include(h => h.Curso)
                .FirstOrDefaultAsync(h => h.IdHorario == idHorario);

            if (horario == null) return NotFound();

            var vm = new ReprogramarClaseViewModel
            {
                IdHorario = horario.IdHorario,
                IdCurso = horario.IdCurso,
                NombreCurso = horario.Curso?.Nombre ?? string.Empty,
                Fecha = horario.Fecha.ToString("yyyy-MM-dd"),
                HoraInicio = $"{horario.HoraInicio.Hours:D2}:{horario.HoraInicio.Minutes:D2}",
                HoraFin = $"{horario.HoraFin.Hours:D2}:{horario.HoraFin.Minutes:D2}"
            };

            return View("~/Views/Perfiles/ReprogramarClase.cshtml", vm);
        }

        // POST: /PerfilProfesor/ReprogramarClase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReprogramarClase(ReprogramarClaseViewModel vm)
        {
            var horario = await _context.Horarios.FindAsync(vm.IdHorario);
            if (horario == null)
            {
                TempData["MensajeError"] = "No se encontró el horario.";
                return RedirectToAction(nameof(MisCursos));
            }

            var horaInicioSpan = TimeSpan.Parse(vm.HoraInicio);
            var horaFinSpan = TimeSpan.Parse(vm.HoraFin);

            if (horaFinSpan <= horaInicioSpan)
            {
                ModelState.AddModelError("", "La hora de fin debe ser mayor a la hora de inicio.");
                vm.NombreCurso = (await _context.Cursos.FindAsync(vm.IdCurso))?.Nombre ?? string.Empty;
                return View("~/Views/Perfiles/ReprogramarClase.cshtml", vm);
            }

            horario.Fecha = DateTime.Parse(vm.Fecha);
            horario.HoraInicio = horaInicioSpan;
            horario.HoraFin = horaFinSpan;

            await _context.SaveChangesAsync();

            var curso = await _context.Cursos.FindAsync(vm.IdCurso);
            var matriculas = await _context.Matriculas
                .Where(m => m.IdCurso == vm.IdCurso && m.Estado == "Activa")
                .ToListAsync();

            foreach (var m in matriculas)
            {
                await NotificacionHelper.EnviarNotificacionAsync(
                    _context, _emailService, m.IdAlumno, "Clase", "USER-09-003",
                    "Cambio de horario",
                    $"La clase del curso '{curso?.Nombre}' fue reprogramada al {DateTime.Parse(vm.Fecha):dd/MM/yyyy} de {vm.HoraInicio} a {vm.HoraFin}.");
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Clase reprogramada y alumnos notificados.";
            return RedirectToAction(nameof(MisCursos));
        }

        // POST: /PerfilProfesor/CancelarClase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarClase(int idHorario, int idCurso)
        {
            var horario = await _context.Horarios.FindAsync(idHorario);
            if (horario == null)
            {
                TempData["MensajeError"] = "No se encontró el horario.";
                return RedirectToAction(nameof(MisCursos));
            }

            var curso = await _context.Cursos.FindAsync(idCurso);
            var fechaHorario = horario.Fecha;
            _context.Horarios.Remove(horario);

            var matriculas = await _context.Matriculas
                .Where(m => m.IdCurso == idCurso && m.Estado == "Activa")
                .ToListAsync();

            foreach (var m in matriculas)
            {
                await NotificacionHelper.EnviarNotificacionAsync(
                    _context, _emailService, m.IdAlumno, "Clase", "USER-09-002",
                    "Clase cancelada",
                    $"La clase del curso '{curso?.Nombre}' programada para el {fechaHorario:dd/MM/yyyy} fue cancelada.");
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Clase cancelada y alumnos notificados.";
            return RedirectToAction(nameof(MisCursos));
        }

        // GET: /PerfilProfesor/MisAlumnos
        public async Task<IActionResult> MisAlumnos(string? buscar, string? curso)
        {
            var profesor = await ObtenerProfesorActual();

            var viewModel = new ProfesorAlumnosViewModel
            {
                Alumnos = new List<AlumnoProfesorListItemViewModel>(),
                FiltroBuscar = buscar,
                FiltroCurso = curso,
                CursosDisponibles = new List<string>()
            };

            if (profesor == null)
            {
                TempData["Error"] = "Tu cuenta no tiene un perfil de profesor vinculado. Contactá al administrador.";
                return View("~/Views/Perfiles/ProfesorAlumnos.cshtml", viewModel);
            }

            var matriculas = await _context.Matriculas
                .Include(m => m.Alumno)
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Horarios)
                .Where(m => m.Estado == "Activa" && m.Curso.IdProfesor == profesor.Id)
                .ToListAsync();

            var alumnos = matriculas.Select(m => new AlumnoProfesorListItemViewModel
            {
                Id = m.IdAlumno,
                NombreCompleto = $"{m.Alumno.Nombre} {m.Alumno.Apellido}",
                Correo = m.Alumno.Email ?? string.Empty,
                Telefono = m.Alumno.PhoneNumber ?? "No registrado",
                Clase = m.Curso.Nombre,
                Activo = !m.Alumno.Bloqueado,
                EstadoMatricula = m.Estado,
                CursosMatriculados = m.Curso.Horarios != null && m.Curso.Horarios.Any()
                    ? m.Curso.Horarios.Select(h =>
                       $"{h.Fecha:dd/MM/yyyy} ({h.DiaSemana}) {h.HoraInicio.Hours:D2}:{h.HoraInicio.Minutes:D2} - {h.HoraFin.Hours:D2}:{h.HoraFin.Minutes:D2}").ToList()
                    : new List<string> { "Sin horario asignado" }
            }).ToList();

            if (!string.IsNullOrWhiteSpace(buscar))
                alumnos = alumnos
                    .Where(a => a.NombreCompleto.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(curso))
                alumnos = alumnos.Where(a => a.Clase == curso).ToList();

            var cursosList = await _context.Cursos
                .Where(c => c.Activo && c.IdProfesor == profesor.Id)
                .OrderBy(c => c.Nombre)
                .Select(c => c.Nombre)
                .ToListAsync();

            viewModel.Alumnos = alumnos.OrderBy(a => a.Clase).ThenBy(a => a.NombreCompleto).ToList();
            viewModel.CursosDisponibles = cursosList;

            return View("~/Views/Perfiles/ProfesorAlumnos.cshtml", viewModel);
        }

        // GET: /PerfilProfesor/Notificaciones
        public async Task<IActionResult> Notificaciones()
        {
            var userId = _userManager.GetUserId(User);

            var notificaciones = await _context.Notificaciones
                .Where(n => n.IdUsuario == userId)
                .OrderByDescending(n => n.FechaEnvio)
                .ToListAsync();

            var model = new NotificacionesProfesorViewModel
            {
                Notificaciones = notificaciones.Select(n => new NotificacionProfesorItemViewModel
                {
                    IdNotificacion = n.IdNotificacion,
                    Tipo = n.Tipo,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    Leida = n.Leida,
                    FechaEnvio = n.FechaEnvio
                }).ToList()
            };

            return View("~/Views/Notificaciones/_NotificacionesProfesor.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarNotificacionLeida(int idNotificacion)
        {
            var userId = _userManager.GetUserId(User);
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion && n.IdUsuario == userId);

            if (notificacion == null)
            {
                TempData["Error"] = "No se encontró la notificación.";
                return RedirectToAction(nameof(Notificaciones));
            }

            notificacion.Leida = true;
            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "Notificación marcada como leída.";
            return RedirectToAction(nameof(Notificaciones));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarNotificacion(int idNotificacion)
        {
            var userId = _userManager.GetUserId(User);
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion && n.IdUsuario == userId);

            if (notificacion == null)
            {
                TempData["Error"] = "No se encontró la notificación.";
                return RedirectToAction(nameof(Notificaciones));
            }

            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "Notificación eliminada.";
            return RedirectToAction(nameof(Notificaciones));
        }
    }
}