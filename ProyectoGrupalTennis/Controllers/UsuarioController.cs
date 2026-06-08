using AcademiaTennisDAL.Context;
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
        // USER-04-002: Alumno visualiza cursos disponibles
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
                        ? $"{c.Profesor.Nombre} {c.Profesor.Apellido}"
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
        // USER-04-001: Matricular cursos
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

            var matricula = new AcademiaTennisDAL.Entities.Matricula
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
        // USER-04-003: Alumno visualiza horarios disponibles
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

            // Lista de días disponibles para el filtro
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
        // USER-04-007: Consultar agenda personal
        public async Task<IActionResult> AgendaPersonal(string? fecha)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            DateTime? fechaSeleccionada = null;

            if (!string.IsNullOrWhiteSpace(fecha))
            {
                fechaSeleccionada = DateTime.Parse(fecha);
            }

            var query =
                from m in _context.Matriculas
                join c in _context.Cursos on m.IdCurso equals c.IdCurso
                join cp in _context.ClasesProgramadas on c.IdCurso equals cp.IdCurso
                where m.IdAlumno == userId
                select new
                {
                    Curso = c.Nombre,
                    Nivel = c.Nivel,
                    FechaClase = cp.FechaClase,
                    HoraInicio = cp.HoraInicio,
                    HoraFin = cp.HoraFin,
                    EstadoClase = cp.Estado
                };

            if (fechaSeleccionada.HasValue)
            {
                query = query.Where(x => x.FechaClase.Date == fechaSeleccionada.Value.Date);
            }

            var clases = await query
                .OrderBy(x => x.FechaClase)
                .ThenBy(x => x.HoraInicio)
                .ToListAsync();

            var viewModel = new AgendaPersonalViewModel
            {
                FiltroFecha = fecha,
                Clases = clases.Select(x => new AgendaPersonalItemViewModel
                {
                    Curso = x.Curso,
                    Nivel = x.Nivel,
                    DiaSemana = x.FechaClase.ToString("dddd"),
                    FechaClase = x.FechaClase.ToString("dd/MM/yyyy"),
                    HoraInicio = x.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin = x.HoraFin.ToString(@"hh\:mm"),
                    EstadoMatricula = x.EstadoClase
                }).ToList()
            };

            return View("~/Views/Matricula/_UsuarioAgenda.cshtml", viewModel);
        }
    }
}