using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    public class CursoController : Controller
    {
        private readonly ICursoService _service;

        public CursoController(ICursoService service)
        {
            _service = service;
        }

        // GET: /Curso/Index
        public IActionResult Index(string? buscar, string? nivel, string? estado)
        {
            var Cursos = _service.ObtenerTodos();

            if (!string.IsNullOrEmpty(buscar))
                Cursos = Cursos
                    .Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(nivel))
                Cursos = Cursos.Where(c => c.Nivel == nivel).ToList();

            if (estado == "Activo")
                Cursos = Cursos.Where(c => c.Activo).ToList();
            else if (estado == "Inactivo")
                Cursos = Cursos.Where(c => !c.Activo).ToList();

            return View("~/Views/Cursos/Index.cshtml", Cursos);
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
                    new HorarioInputViewModel() // un horario vacío por defecto
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
            var Curso = _service.ObtenerPorId(id);
            if (Curso == null) return NotFound();

            var vm = new CursoFormViewModel
            {
                Curso = Curso,
                Profesores = _service.ObtenerProfesores(),
                Horarios = Curso.Horarios.Select(h => new HorarioInputViewModel
                {
                    IdHorario = h.IdHorario,
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin = h.HoraFin.ToString(@"hh\:mm")
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
                return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        // Helper: convierte los inputs de horario en entidades Horario
        private List<Horario> MapearHorarios(List<HorarioInputViewModel> inputs)
        {
            var horarios = new List<Horario>();
            foreach (var h in inputs)
            {
                if (string.IsNullOrWhiteSpace(h.DiaSemana)) continue;

                horarios.Add(new Horario
                {
                    IdHorario = h.IdHorario,
                    DiaSemana = h.DiaSemana,
                    HoraInicio = TimeSpan.Parse(h.HoraInicio),
                    HoraFin = TimeSpan.Parse(h.HoraFin)
                });
            }
            return horarios;
        }
    }
}