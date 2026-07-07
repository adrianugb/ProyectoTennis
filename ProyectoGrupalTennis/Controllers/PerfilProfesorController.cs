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

            // Si no hay profesor vinculado, mostrar lista vacía con mensaje
            // en vez de redirigir a Home
            var viewModel = new ProfesorCursosViewModel
            {
                FiltroBuscar = buscar,
                FiltroNivel = nivel,
                Cursos = new List<CursoListItemViewModel>()
            };

            if (profesor == null)
            {
                TempData["Error"] = "Tu cuenta no tiene un perfil de profesor vinculado. Contactá al administrador.";
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

            viewModel.Cursos = cursos.Select(c => new CursoListItemViewModel
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
            }).ToList();

            return View("~/Views/Perfiles/ProfesorCursos.cshtml", viewModel);
        }

        // GET: /PerfilProfesor/MisAlumnos
        // Usa Matriculas + Horarios del curso en vez de ClasesProgramadas
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

            // Trae matrículas activas de cursos del profesor, con horarios
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
                // Horarios del curso como texto
                CursosMatriculados = m.Curso.Horarios != null && m.Curso.Horarios.Any()
                    ? m.Curso.Horarios.Select(h =>
                        $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
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

        // POST: /PerfilProfesor/MarcarNotificacionLeida
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

        // POST: /PerfilProfesor/EliminarNotificacion
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