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
        public async Task<IActionResult> AgendaPersonal(string? dia, string? fecha)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var query =
                from m in _context.Matriculas
                join c in _context.Cursos on m.IdCurso equals c.IdCurso
                join h in _context.Horarios on c.IdCurso equals h.IdCurso
                where m.IdAlumno == userId
                select new AgendaPersonalItemViewModel
                {
                    Curso = c.Nombre,
                    Nivel = c.Nivel,
                    DiaSemana = h.DiaSemana,
                    FechaClase = "",
                    HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin = h.HoraFin.ToString(@"hh\:mm"),
                    EstadoMatricula = m.Estado
                };
            if (!string.IsNullOrWhiteSpace(dia))
            {
                query = query.Where(x => x.DiaSemana == dia);
            }

            var diasDisponibles = await _context.Horarios
                .Select(h => h.DiaSemana)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var viewModel = new AgendaPersonalViewModel
            {
                FiltroDia = dia,
                FiltroFecha = fecha,
                DiasDisponibles = diasDisponibles,
                Clases = await query.ToListAsync()
            };

            return View("~/Views/Matricula/_UsuarioAgenda.cshtml", viewModel);
        }
    }
}