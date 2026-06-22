using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Usuario/MisCursos
        public async Task<IActionResult> MisCursos(string? buscar, string? nivel)
        {
            var query = _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Profesor)
                .Where(c => c.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(c => c.Nombre.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(nivel))
                query = query.Where(c => c.Nivel == nivel);

            var cursos = await query.OrderBy(c => c.Nombre).ToListAsync();

            var viewModel = new UsuarioCursosViewModel
            {
                FiltroBuscar = buscar,
                FiltroNivel = nivel,
                Cursos = cursos.Select(c => new CursoUsuarioItemViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion ?? string.Empty,
                    Nivel = c.Nivel,
                    CuposDisponibles = c.CuposDisponibles,
                    NombreProfesor = c.Profesor != null
                        ? $"{c.Profesor.Nombre} {c.Profesor.Apellidos}"
                        : "Sin asignar",
                    Horarios = c.Horarios != null
                        ? c.Horarios.Select(h =>
                            $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
                        : new List<string>()
                }).ToList()
            };

            return View("~/Views/Perfiles/UsuarioCursos.cshtml", viewModel);
        }

        // POST: /Usuario/MatricularCurso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MatricularCurso(int idCurso)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Debe iniciar sesión para matricular un curso.";
                return RedirectToAction("Login", "Auth");
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCurso && c.Activo);

            if (curso == null)
            {
                TempData["Error"] = "El curso seleccionado no existe o no está activo.";
                return RedirectToAction("MisCursos");
            }

            if (curso.CuposDisponibles <= 0)
            {
                TempData["Error"] = "No hay cupos disponibles para este curso.";
                return RedirectToAction("MisCursos");
            }

            var yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.IdAlumno == userId &&
                               m.IdCurso == idCurso &&
                               m.Estado == "Activa");

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("MisCursos");
            }

            var matricula = new Matricula
            {
                IdAlumno = userId,
                IdCurso = idCurso,
                FechaMatricula = DateTime.Now,
                Estado = "Activa"
            };

            _context.Matriculas.Add(matricula);
            curso.CuposDisponibles -= 1;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula realizada correctamente.";
            return RedirectToAction("MisCursos");
        }

        // GET: /Usuario/MisHorarios
        public async Task<IActionResult> MisHorarios(string? buscar, string? dia)
        {
            var query = _context.Horarios
                .Include(h => h.Curso)
                .Where(h => h.Curso.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(h => h.Curso.Nombre.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(dia))
                query = query.Where(h => h.DiaSemana == dia);

            var horarios = await query.OrderBy(h => h.DiaSemana)
                                      .ThenBy(h => h.HoraInicio)
                                      .ToListAsync();

            var dias = await _context.Horarios
                .Select(h => h.DiaSemana)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var viewModel = new UsuarioHorariosViewModel
            {
                FiltroBuscar = buscar,
                FiltroDia = dia,
                DiasDisponibles = dias,
                Horarios = horarios.Select(h => new HorarioUsuarioItemViewModel
                {
                    IdHorario = h.IdHorario,
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin = h.HoraFin.ToString(@"hh\:mm"),
                    NombreCurso = h.Curso.Nombre,
                    Nivel = h.Curso.Nivel,
                    CuposDisponibles = h.Curso.CuposDisponibles
                }).ToList()
            };

            return View("~/Views/Perfiles/UsuarioHorarios.cshtml", viewModel);
        }

        // GET: /Usuario/AgendaPersonal
        public async Task<IActionResult> AgendaPersonal(string? dia)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Trae las matrículas activas del usuario logueado, junto con el curso
            // y sus horarios recurrentes (Curso -> Horarios).
            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Horarios)
                .Where(m => m.IdAlumno == userId && m.Estado == "Activa")
                .ToListAsync();

            var clases = new List<AgendaPersonalItemViewModel>();

            foreach (var m in matriculas)
            {
                if (m.Curso == null) continue;

                if (m.Curso.Horarios != null && m.Curso.Horarios.Any())
                {
                    // Una fila por cada horario recurrente del curso matriculado
                    foreach (var h in m.Curso.Horarios)
                    {
                        clases.Add(new AgendaPersonalItemViewModel
                        {
                            IdMatricula = m.IdMatricula,
                            IdCurso = m.Curso.IdCurso,
                            Curso = m.Curso.Nombre,
                            Nivel = m.Curso.Nivel,
                            DiaSemana = h.DiaSemana,
                            FechaClase = string.Empty, // recurrente, no tiene fecha concreta
                            HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
                            HoraFin = h.HoraFin.ToString(@"hh\:mm"),
                            EstadoMatricula = m.Estado
                        });
                    }
                }
                else
                {
                    // El curso aún no tiene horarios cargados; igual se muestra la matrícula
                    clases.Add(new AgendaPersonalItemViewModel
                    {
                        IdMatricula = m.IdMatricula,
                        IdCurso = m.Curso.IdCurso,
                        Curso = m.Curso.Nombre,
                        Nivel = m.Curso.Nivel,
                        DiaSemana = "Sin horario asignado",
                        FechaClase = string.Empty,
                        HoraInicio = string.Empty,
                        HoraFin = string.Empty,
                        EstadoMatricula = m.Estado
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(dia))
            {
                clases = clases.Where(x => x.DiaSemana == dia).ToList();
            }

            var diasDisponibles = clases
                .Select(x => x.DiaSemana)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var viewModel = new AgendaPersonalViewModel
            {
                FiltroDia = dia,
                DiasDisponibles = diasDisponibles,
                Clases = clases
                    .OrderBy(x => x.DiaSemana)
                    .ThenBy(x => x.HoraInicio)
                    .ToList()
            };

            return View("~/Views/Matricula/_UsuarioAgenda.cshtml", viewModel);
        }
        // POST: /Usuario/CancelarMatricula
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int idMatricula)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var matricula = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatricula && m.IdAlumno == userId);

            if (matricula == null)
            {
                TempData["Error"] = "No se encontró la matrícula indicada.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            matricula.Estado = "Cancelada";

            var curso = await _context.Cursos.FindAsync(matricula.IdCurso);
            if (curso != null)
            {
                curso.CuposDisponibles += 1;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula cancelada correctamente.";
            return RedirectToAction(nameof(AgendaPersonal));
        }

        // GET: /Usuario/Reprogramar/5
        public async Task<IActionResult> Reprogramar(int idMatricula)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var matricula = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatricula && m.IdAlumno == userId);

            if (matricula == null)
            {
                TempData["Error"] = "No se encontró la matrícula indicada.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            var cursos = await _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Profesor)
                .Where(c => c.Activo && c.IdCurso != matricula.IdCurso)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var viewModel = new UsuarioCursosViewModel
            {
                Cursos = cursos.Select(c => new CursoUsuarioItemViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion ?? string.Empty,
                    Nivel = c.Nivel,
                    CuposDisponibles = c.CuposDisponibles,
                    NombreProfesor = c.Profesor != null
                        ? $"{c.Profesor.Nombre} {c.Profesor.Apellidos}"
                        : "Sin asignar",
                    Horarios = c.Horarios != null
                        ? c.Horarios.Select(h =>
                            $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
                        : new List<string>()
                }).ToList()
            };

            ViewBag.IdMatriculaOrigen = idMatricula;

            return View("~/Views/Cursos/Reprogramar.cshtml", viewModel);
        }

        // POST: /Usuario/ConfirmarReprogramacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarReprogramacion(int idMatriculaOrigen, int idCursoNuevo)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var matriculaOrigen = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatriculaOrigen && m.IdAlumno == userId);

            if (matriculaOrigen == null)
            {
                TempData["Error"] = "No se encontró la matrícula original.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            var cursoNuevo = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCursoNuevo && c.Activo);

            if (cursoNuevo == null)
            {
                TempData["Error"] = "El curso seleccionado no existe o no está activo.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            if (cursoNuevo.CuposDisponibles <= 0)
            {
                TempData["Error"] = "No hay cupos disponibles en el curso seleccionado.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            var yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.IdAlumno == userId &&
                               m.IdCurso == idCursoNuevo &&
                               m.Estado == "Activa");

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado en ese curso.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            matriculaOrigen.Estado = "Cancelada";
            var cursoOrigen = await _context.Cursos.FindAsync(matriculaOrigen.IdCurso);
            if (cursoOrigen != null)
            {
                cursoOrigen.CuposDisponibles += 1;
            }

            var nuevaMatricula = new Matricula
            {
                IdAlumno = userId,
                IdCurso = idCursoNuevo,
                FechaMatricula = DateTime.Now,
                Estado = "Activa"
            };
            _context.Matriculas.Add(nuevaMatricula);

            cursoNuevo.CuposDisponibles -= 1;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Te reprogramaste exitosamente al curso '{cursoNuevo.Nombre}'.";
            return RedirectToAction(nameof(AgendaPersonal));
        }
    }
}