using AcademiaTennisDAL.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using AcademiaTennisDAL.Entities;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Profesor")]
    public class PerfilProfesorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PerfilProfesorController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /PerfilProfesor/MisCursos
        // Historia 5: Profesor visualiza cursos del catálogo
        public async Task<IActionResult> MisCursos(string? buscar, string? nivel)
        {
            // Traer todos los cursos con sus horarios
            var query = _context.Cursos
                .Include(c => c.Horarios)
                .AsQueryable();

            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(c => c.Nombre.Contains(buscar));

            // Filtro por nivel
            if (!string.IsNullOrWhiteSpace(nivel))
                query = query.Where(c => c.Nivel == nivel);

            var cursos = await query.OrderBy(c => c.Nombre).ToListAsync();

            var viewModel = new ProfesorCursosViewModel
            {
                FiltroBuscar = buscar,
                FiltroNivel = nivel,
                Cursos = cursos.Select(c => new CursoListItemViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion ?? string.Empty,
                    Nivel = c.Nivel,
                    CuposDisponibles = c.CuposDisponibles,
                    Activo = c.Activo,
                    Horarios = c.Horarios != null
                        ? c.Horarios.Select(h =>
                            $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
                        : new List<string>()
                }).ToList()
            };

            return View("~/Views/Perfiles/ProfesorCursos.cshtml", viewModel);
        }

        // GET: /PerfilProfesor/MisAlumnos
        // Historia 6: Profesor visualiza alumnos matriculados
        public async Task<IActionResult> MisAlumnos(string? buscar, string? curso)
        {
            // Traer todos los alumnos con rol "Usuario" que tengan al menos una matrícula
            var matriculas = await _context.Matriculas
                .Include(m => m.Alumno)
                .Include(m => m.Curso)
                .ToListAsync();

            // Agrupar por alumno
            var alumnosAgrupados = matriculas
                .GroupBy(m => m.IdAlumno)
                .Select(g => new AlumnoProfesorListItemViewModel
                {
                    Id = g.Key,
                    NombreCompleto = $"{g.First().Alumno.Nombre} {g.First().Alumno.Apellido}",
                    Correo = g.First().Alumno.Email ?? string.Empty,
                    Telefono = g.First().Alumno.PhoneNumber ?? "No registrado",
                    Activo = !g.First().Alumno.Bloqueado,
                    CursosMatriculados = g.Select(m => m.Curso.Nombre).Distinct().ToList()
                })
                .ToList();

            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(buscar))
                alumnosAgrupados = alumnosAgrupados
                    .Where(a => a.NombreCompleto.Contains(buscar, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            // Filtro por curso
            if (!string.IsNullOrWhiteSpace(curso))
                alumnosAgrupados = alumnosAgrupados
                    .Where(a => a.CursosMatriculados.Contains(curso, StringComparer.OrdinalIgnoreCase))
                    .ToList();

            // Lista de cursos para el select del filtro
            var cursosList = await _context.Cursos
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => c.Nombre)
                .ToListAsync();

            var viewModel = new ProfesorAlumnosViewModel
            {
                Alumnos = alumnosAgrupados.OrderBy(a => a.NombreCompleto).ToList(),
                FiltroBuscar = buscar,
                FiltroCurso = curso,
                CursosDisponibles = cursosList
            };

            return View("~/Views/Perfiles/ProfesorAlumnos.cshtml", viewModel);
        }
    }
}