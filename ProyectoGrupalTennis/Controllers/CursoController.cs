using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Helpers;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;

namespace ProyectoGrupalTennis.Controllers
{
    public class CursoController : Controller
    {
        private readonly ICursoService _service;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly GoogleCalendarService _calendarService;

        public CursoController(
            ICursoService service,
            AppDbContext context,
            EmailService emailService,
            GoogleCalendarService calendarService)
        {
            _service = service;
            _context = context;
            _emailService = emailService;
            _calendarService = calendarService;
        }

        public IActionResult Index(string? buscar, string? nivel, string? estado)
        {
            var cursos = _service.ObtenerTodos();
            if (!string.IsNullOrEmpty(buscar))
                cursos = cursos.Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(nivel))
                cursos = cursos.Where(c => c.Nivel == nivel).ToList();
            if (estado == "Activo")
                cursos = cursos.Where(c => c.Activo).ToList();
            else if (estado == "Inactivo")
                cursos = cursos.Where(c => !c.Activo).ToList();
            return View("~/Views/Cursos/Index.cshtml", cursos);
        }

        public IActionResult Agregar()
        {
            var vm = new CursoFormViewModel
            {
                Curso = new Curso(),
                Profesores = _service.ObtenerProfesores(),
                Horarios = new List<HorarioInputViewModel> { new HorarioInputViewModel() }
            };
            return View("~/Views/Cursos/Agregar.cshtml", vm);
        }

        // POST: /Curso/Agregar
        [HttpPost]
        public async Task<IActionResult> Agregar(CursoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Agregar.cshtml", vm);
            }

            try
            {
                var horarios = MapearHorarios(vm.Horarios);
                _service.Agregar(vm.Curso, horarios);

                // ── Google Calendar: crear evento en el calendario del admin ──
                var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (adminUserId != null && await _calendarService.TieneTokenAsync(adminUserId))
                {
                    foreach (var h in horarios)
                    {
                        var eventId = await _calendarService.CrearEventoAsync(
                            adminUserId,
                            $"Clase: {vm.Curso.Nombre}",
                            $"Nivel: {vm.Curso.Nivel}",
                            h.Fecha, h.HoraInicio, h.HoraFin);

                        if (eventId != null)
                        {
                            h.GoogleEventId = eventId;
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // ── Google Calendar: crear evento en el calendario del profesor asignado ──
                if (vm.Curso.IdProfesor.HasValue)
                {
                    var profesor = await _context.Profesores
                        .FirstOrDefaultAsync(p => p.Id == vm.Curso.IdProfesor.Value);

                    if (profesor?.UserId != null && await _calendarService.TieneTokenAsync(profesor.UserId))
                    {
                        foreach (var h in horarios)
                        {
                            await _calendarService.CrearEventoAsync(
                                profesor.UserId,
                                $"Clase a impartir: {vm.Curso.Nombre}",
                                $"Nivel: {vm.Curso.Nivel}",
                                h.Fecha, h.HoraInicio, h.HoraFin);
                        }
                    }
                }

                return RedirectToAction("Index", "Curso");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Agregar.cshtml", vm);
            }
        }

        public IActionResult Editar(int id)
        {
            var curso = _service.ObtenerPorId(id);
            if (curso == null) return NotFound();

            var vm = new CursoFormViewModel
            {
                Curso = curso,
                Profesores = _service.ObtenerProfesores(),
                Horarios = curso.Horarios.Select(h => new HorarioInputViewModel
                {
                    IdHorario = h.IdHorario,
                    Fecha = h.Fecha != DateTime.MinValue ? h.Fecha.ToString("yyyy-MM-dd") : string.Empty,
                    HoraInicio = $"{h.HoraInicio.Hours:D2}:{h.HoraInicio.Minutes:D2}",
                    HoraFin = $"{h.HoraFin.Hours:D2}:{h.HoraFin.Minutes:D2}"
                }).ToList()
            };

            if (vm.Horarios.Count == 0)
                vm.Horarios.Add(new HorarioInputViewModel());

            return View("~/Views/Cursos/Editar.cshtml", vm);
        }

        // POST: /Curso/Editar
        [HttpPost]
        public async Task<IActionResult> Editar(CursoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Editar.cshtml", vm);
            }

            try
            {
                // Guardar GoogleEventIds existentes antes de que Actualizar los reemplace
                var horariosAnteriores = await _context.Horarios
                    .Where(h => h.IdCurso == vm.Curso.IdCurso)
                    .ToListAsync();

                var horarios = MapearHorarios(vm.Horarios);
                _service.Actualizar(vm.Curso, horarios);

                // ── Google Calendar: actualizar eventos del admin ──
                var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (adminUserId != null && await _calendarService.TieneTokenAsync(adminUserId))
                {
                    // Eliminar eventos anteriores
                    foreach (var hAnterior in horariosAnteriores.Where(h => h.GoogleEventId != null))
                    {
                        await _calendarService.EliminarEventoAsync(adminUserId, hAnterior.GoogleEventId!);
                    }

                    // Crear nuevos eventos
                    var horariosNuevos = await _context.Horarios
                        .Where(h => h.IdCurso == vm.Curso.IdCurso)
                        .ToListAsync();

                    foreach (var h in horariosNuevos)
                    {
                        var eventId = await _calendarService.CrearEventoAsync(
                            adminUserId,
                            $"Clase: {vm.Curso.Nombre}",
                            $"Nivel: {vm.Curso.Nivel}",
                            h.Fecha, h.HoraInicio, h.HoraFin);

                        if (eventId != null)
                        {
                            h.GoogleEventId = eventId;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Curso");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Editar.cshtml", vm);
            }
        }

        [HttpPost]
        public IActionResult CambiarEstado(int id, bool activo)
        {
            _service.CambiarEstado(id, activo);
           return RedirectToAction("Index", "Curso");
        }

        [HttpPost]
        public async Task<IActionResult> AgregarHorario(CursoFormViewModel vm)
        {
            vm.NuevoHorario.IdCurso = vm.Curso.IdCurso;
            try
            {
                _service.AgregarHorario(vm.NuevoHorario);

                var matriculas = await _context.Matriculas
                    .Where(m => m.IdCurso == vm.Curso.IdCurso && m.Estado == "Activa")
                    .ToListAsync();

                var curso = await _context.Cursos
                    .Include(c => c.Profesor)
                    .FirstOrDefaultAsync(c => c.IdCurso == vm.Curso.IdCurso);

                foreach (var m in matriculas)
                {
                    await NotificacionHelper.EnviarNotificacionAsync(
                        _context, _emailService, m.IdAlumno, "Clase", "USER-09-003",
                        "Cambio de horario",
                        $"El horario del curso '{curso?.Nombre}' fue actualizado. Revisá tu agenda.");
                }

                if (curso?.Profesor?.UserId != null)
                {
                    await NotificacionHelper.EnviarNotificacionAsync(
                        _context, _emailService, curso.Profesor.UserId, "Clase", "PROF-09-002",
                        "Cambio de horario en tu curso",
                        $"El horario del curso '{curso.Nombre}' que impartís fue modificado.");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction(nameof(Editar), new { id = vm.Curso.IdCurso });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarHorario(int idHorario, int idCurso)
        {
            try
            {
                // Guardar GoogleEventId antes de eliminar
                var horario = await _context.Horarios.FindAsync(idHorario);
                var googleEventId = horario?.GoogleEventId;

                _service.EliminarHorario(idHorario);

                var matriculas = await _context.Matriculas
                    .Where(m => m.IdCurso == idCurso && m.Estado == "Activa")
                    .ToListAsync();

                var curso = await _context.Cursos
                    .Include(c => c.Profesor)
                    .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

                foreach (var m in matriculas)
                {
                    await NotificacionHelper.EnviarNotificacionAsync(
                        _context, _emailService, m.IdAlumno, "Clase", "USER-09-003",
                        "Cambio de horario",
                        $"El horario del curso '{curso?.Nombre}' fue actualizado. Revisá tu agenda.");
                }

                if (curso?.Profesor?.UserId != null)
                {
                    await NotificacionHelper.EnviarNotificacionAsync(
                        _context, _emailService, curso.Profesor.UserId, "Clase", "PROF-09-002",
                        "Cambio de horario en tu curso",
                        $"El horario del curso '{curso.Nombre}' que impartís fue modificado.");

                    // ── Google Calendar: eliminar evento del profesor ──
                    if (googleEventId != null && await _calendarService.TieneTokenAsync(curso.Profesor.UserId))
                    {
                        await _calendarService.EliminarEventoAsync(curso.Profesor.UserId, googleEventId);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction(nameof(Editar), new { id = idCurso });
        }

        private List<Horario> MapearHorarios(List<HorarioInputViewModel> inputs)
        {
            var horarios = new List<Horario>();
            foreach (var h in inputs)
            {
                if (string.IsNullOrWhiteSpace(h.Fecha)) continue;
                if (string.IsNullOrWhiteSpace(h.HoraInicio)) continue;
                if (string.IsNullOrWhiteSpace(h.HoraFin)) continue;

                horarios.Add(new Horario
                {
                    IdHorario = h.IdHorario,
                    Fecha = DateTime.Parse(h.Fecha),
                    HoraInicio = TimeSpan.Parse(h.HoraInicio),
                    HoraFin = TimeSpan.Parse(h.HoraFin)
                });
            }
            return horarios;
        }
    }
}