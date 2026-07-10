using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Helpers;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    public class CursoController : Controller
    {
        private readonly ICursoService _service;
        private readonly AppDbContext _context;
        private readonly ProyectoGrupalTennis.Services.EmailService _emailService;

        public CursoController(ICursoService service, AppDbContext context, ProyectoGrupalTennis.Services.EmailService emailService)
        {
            _service = service;
            _context = context;
            _emailService = emailService;
        }

        // GET: /Curso/Index
        public IActionResult Index(string? buscar, string? nivel, string? estado)
        {
            var cursos = _service.ObtenerTodos();

            if (!string.IsNullOrEmpty(buscar))
                cursos = cursos
                    .Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(nivel))
                cursos = cursos.Where(c => c.Nivel == nivel).ToList();

            if (estado == "Activo")
                cursos = cursos.Where(c => c.Activo).ToList();
            else if (estado == "Inactivo")
                cursos = cursos.Where(c => !c.Activo).ToList();

            return View("~/Views/Cursos/Index.cshtml", cursos);
        }

        // GET: /Curso/Agregar
        public IActionResult Agregar()
        {
            var vm = new CursoFormViewModel
            {
                Curso = new Curso(),
                Profesores = _service.ObtenerProfesores(),
                Horarios = new List<HorarioInputViewModel>
                {
                    new HorarioInputViewModel()
                }
            };
            return View("~/Views/Cursos/Agregar.cshtml", vm);
        }

        // POST: /Curso/Agregar
        [HttpPost]
        public IActionResult Agregar(CursoFormViewModel vm)
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
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Agregar.cshtml", vm);
            }
        }

        // GET: /Curso/Editar/5
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
                    Fecha = h.Fecha.ToString("yyyy-MM-dd"), // ← formato que espera input type="date"
                    HoraInicio = h.HoraInicio.ToString(@"HH\:mm"),
                    HoraFin = h.HoraFin.ToString(@"HH\:mm")
                }).ToList()
            };

            if (vm.Horarios.Count == 0)
                vm.Horarios.Add(new HorarioInputViewModel());

            return View("~/Views/Cursos/Editar.cshtml", vm);
        }

        // POST: /Curso/Editar
        [HttpPost]
        public IActionResult Editar(CursoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Editar.cshtml", vm);
            }

            try
            {
                var horarios = MapearHorarios(vm.Horarios);
                _service.Actualizar(vm.Curso, horarios);
                return RedirectToAction("Index", "AdminCursos");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.Profesores = _service.ObtenerProfesores();
                return View("~/Views/Cursos/Editar.cshtml", vm);
            }
        }

        // POST: /Curso/CambiarEstado
        [HttpPost]
        public IActionResult CambiarEstado(int id, bool activo)
        {
            _service.CambiarEstado(id, activo);
            return RedirectToAction("Index", "AdminCursos");
        }

        // POST: /Curso/AgregarHorario
        [HttpPost]
        public async Task<IActionResult> AgregarHorario(CursoFormViewModel vm)
        {
            vm.NuevoHorario.IdCurso = vm.Curso.IdCurso;
            try
            {
                _service.AgregarHorario(vm.NuevoHorario);

                // Notificar alumnos matriculados (USER-09-003)
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

                // Notificar al profesor (PROF-09-002)
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

        // POST: /Curso/EliminarHorario
        [HttpPost]
        public async Task<IActionResult> EliminarHorario(int idHorario, int idCurso)
        {
            try
            {
                _service.EliminarHorario(idHorario);

                // Notificar alumnos matriculados (USER-09-003)
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

                // Notificar al profesor (PROF-09-002)
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

            return RedirectToAction(nameof(Editar), new { id = idCurso });
        }

        // Helper
        
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