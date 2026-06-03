using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminCursosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminCursosController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /AdminCursos/Index
        // ADM-04-006: Visualizar cursos con profesor asignado
        public async Task<IActionResult> Index(string? buscar)
        {
            var cursos = await _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Profesor)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(buscar))
                cursos = cursos
                    .Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            // Obtener todos los usuarios con rol Profesor
            var profesores = await _userManager.GetUsersInRoleAsync("Profesor");

            var viewModel = new AdminCursosViewModel
            {
                FiltroBuscar = buscar,
                MensajeExito = TempData["MensajeExito"]?.ToString(),
                MensajeError = TempData["MensajeError"]?.ToString(),
                Profesores = profesores.Select(p => new ProfesorSelectViewModel
                {
                    Id = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellido}"
                }).ToList(),
                Cursos = cursos.Select(c => new CursoAdminItemViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre,
                    Nivel = c.Nivel,
                    Activo = c.Activo,
                    NombreProfesor = c.Profesor != null
                        ? $"{c.Profesor.Nombre} {c.Profesor.Apellido}"
                        : null,
                    Horarios = c.Horarios != null
                        ? c.Horarios.Select(h =>
                            $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
                        : new List<string>()
                }).ToList()
            };

            return View("~/Views/Perfiles/AdminCursos.cshtml", viewModel);
        }

        // GET: /AdminCursos/ObtenerProfesor/5
        // AJAX: obtener profesor asignado a un curso
        [HttpGet]
        public async Task<IActionResult> ObtenerProfesor(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Profesor)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
                return Json(new { exito = false, mensaje = "El curso no existe." });

            return Json(new
            {
                exito = true,
                idCurso = curso.IdCurso,
                nombreCurso = curso.Nombre,
                idProfesorActual = curso.IdProfesorUserId ?? ""
            });
        }

        // POST: /AdminCursos/AsignarProfesor
        // ADM-04-006: Asignar profesor a curso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarProfesor(int idCurso, string idProfesor)
        {
            var curso = await _context.Cursos.FindAsync(idCurso);

            if (curso == null)
            {
                TempData["MensajeError"] = "El curso no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que el profesor existe y tiene rol Profesor
            var profesor = await _userManager.FindByIdAsync(idProfesor);
            if (profesor == null || !await _userManager.IsInRoleAsync(profesor, "Profesor"))
            {
                TempData["MensajeError"] = "El profesor seleccionado no es válido.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar conflicto de horario:
            // Un profesor no puede tener dos cursos con horarios que se solapan
            var horariosNuevoCurso = await _context.Horarios
                .Where(h => h.IdCurso == idCurso)
                .ToListAsync();

            var cursosProfesor = await _context.Cursos
                .Include(c => c.Horarios)
                .Where(c => c.IdProfesorUserId == idProfesor && c.IdCurso != idCurso)
                .ToListAsync();

            foreach (var cursoExistente in cursosProfesor)
            {
                foreach (var horarioExistente in cursoExistente.Horarios)
                {
                    foreach (var horarioNuevo in horariosNuevoCurso)
                    {
                        if (horarioExistente.DiaSemana == horarioNuevo.DiaSemana &&
                            horarioExistente.HoraInicio < horarioNuevo.HoraFin &&
                            horarioExistente.HoraFin > horarioNuevo.HoraInicio)
                        {
                            TempData["MensajeError"] =
                                $"El profesor tiene un conflicto de horario con el curso '{cursoExistente.Nombre}' " +
                                $"el {horarioExistente.DiaSemana} de {horarioExistente.HoraInicio:hh\\:mm} a {horarioExistente.HoraFin:hh\\:mm}.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }

            curso.IdProfesorUserId = idProfesor;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                $"Profesor '{profesor.Nombre} {profesor.Apellido}' asignado al curso '{curso.Nombre}' correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminCursos/QuitarProfesor
        // Quitar profesor asignado a un curso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarProfesor(int idCurso)
        {
            var curso = await _context.Cursos.FindAsync(idCurso);

            if (curso == null)
            {
                TempData["MensajeError"] = "El curso no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            curso.IdProfesorUserId = null;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Profesor removido del curso '{curso.Nombre}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}