using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;
using System.Security.Claims;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class SolicitudesCursoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public SolicitudesCursoController(
            AppDbContext context,
            EmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        // GET: /SolicitudesCurso/Catalogo
        [HttpGet]
        public IActionResult Catalogo()
        {
            var model = new CatalogoCursosViewModel
            {
                Cursos = new List<CatalogoCursoItemViewModel>
                {
                    new()
                    {
                        Nombre = "Clases Esporádicas",
                        Descripcion = "Clases para alumnos que desean asistir de forma ocasional con reservación previa.",
                        Detalle = "Horario a convenir según disponibilidad de la academia.",
                        Imagen = "/images/clases-esporadicas.jpg"
                    },
                    new()
                    {
                        Nombre = "Curso Intensivo",
                        Descripcion = "Paquete de 10 lecciones impartidas en un máximo de 2 semanas.",
                        Detalle = "Ideal para avanzar rápidamente. Los horarios se coordinan con la academia.",
                        Imagen = "/images/curso-intensivo.jpg"
                    },
                    new()
                    {
                        Nombre = "Clases Específicas",
                        Descripcion = "Clases enfocadas en mejorar una técnica o tema específico.",
                        Detalle = "El alumno indica sus necesidades y disponibilidad.",
                        Imagen = "/images/clases-especificas.jpg"
                    },
                    new()
                    {
                        Nombre = "Clases a Domicilio",
                        Descripcion = "Clases en el lugar de residencia si el alumno cuenta con cancha.",
                        Detalle = "Modalidad sujeta a coordinación previa.",
                        Imagen = "/images/clases-domicilio.jpg"
                    },
                    new()
                    {
                        Nombre = "Paquete Empresarial",
                        Descripcion = "Paquete especial para empresas y colaboradores.",
                        Detalle = "Requiere coordinación con la academia.",
                        Imagen = "/images/paquete-empresarial.jpg"
                    },
                    new()
                    {
                        Nombre = "Clases para Jubilados",
                        Descripcion = "Clases especializadas para personas jubiladas y de la tercera edad.",
                        Detalle = "Puede ser individual, pareja o grupo.",
                        Imagen = "/images/clases-jubilados.jpg"
                    }
                }
            };

            return View("~/Views/Perfiles/CatalogoCursos.cshtml", model);
        }

        // GET: /SolicitudesCurso/Crear
        [HttpGet]
        public IActionResult Crear(string nombreCurso)
        {
            if (string.IsNullOrWhiteSpace(nombreCurso))
            {
                TempData["Error"] = "Debe seleccionar un tipo de clase.";
                return RedirectToAction(nameof(Catalogo));
            }

            var model = new SolicitudCursoViewModel
            {
                NombreCurso = nombreCurso,
                Disponibilidades = new List<DisponibilidadSolicitudViewModel>
                {
                    new()
                }
            };

            return View("~/Views/Perfiles/SolicitarCurso.cshtml", model);
        }

        // POST: /SolicitudesCurso/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(SolicitudCursoViewModel model)
        {
            model.Disponibilidades ??= new List<DisponibilidadSolicitudViewModel>();

            if (model.EsADomicilio &&
                string.IsNullOrWhiteSpace(model.DireccionDomicilio))
            {
                ModelState.AddModelError(
                    nameof(model.DireccionDomicilio),
                    "Debe indicar la dirección para la clase a domicilio.");
            }

            if (!model.Disponibilidades.Any())
            {
                ModelState.AddModelError(
                    nameof(model.Disponibilidades),
                    "Debe agregar al menos un horario disponible.");
            }

            foreach (var disponibilidad in model.Disponibilidades)
            {
                if (disponibilidad.HoraHasta <= disponibilidad.HoraDesde)
                {
                    ModelState.AddModelError(
                        nameof(model.Disponibilidades),
                        "La hora final debe ser posterior a la hora inicial.");
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Perfiles/SolicitarCurso.cshtml", model);
            }

            var idAlumno = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Se conserva el campo anterior para compatibilidad con las vistas existentes.
            var disponibilidadResumen = string.Join(
                "; ",
                model.Disponibilidades.Select(d =>
                    $"{d.DiaSemana}: {d.HoraDesde:hh\\:mm} - {d.HoraHasta:hh\\:mm}"));

            var solicitud = new SolicitudCurso
            {
                IdAlumno = idAlumno,
                IdCurso = model.IdCurso,
                NombreCurso = model.NombreCurso,
                Nivel = model.Nivel,
                Modalidad = model.Modalidad,
                CantidadLecciones = model.CantidadLecciones,
                RequiereEquipo = model.RequiereEquipo,
                EsADomicilio = model.EsADomicilio,
                DireccionDomicilio = model.EsADomicilio
                    ? model.DireccionDomicilio
                    : null,
                Disponibilidad = disponibilidadResumen,
                Comentarios = model.Comentarios,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.Now,
                Disponibilidades = model.Disponibilidades
                    .Select(d => new DisponibilidadSolicitud
                    {
                        DiaSemana = d.DiaSemana,
                        HoraDesde = d.HoraDesde,
                        HoraHasta = d.HoraHasta
                    })
                    .ToList()
            };

            _context.SolicitudesCurso.Add(solicitud);
            await _context.SaveChangesAsync();

            var correoDestino =
                _configuration["AcademiaSettings:CorreoSolicitudes"];

            if (!string.IsNullOrWhiteSpace(correoDestino))
            {
                await _emailService.EnviarCorreoAsync(
                    correoDestino,
                    $"Nueva solicitud SOL-{solicitud.IdSolicitudCurso}",
                    $"""
                    <strong>Solicitud:</strong> SOL-{solicitud.IdSolicitudCurso}<br>
                    <strong>Clase:</strong> {solicitud.NombreCurso}<br>
                    <strong>Nivel:</strong> {solicitud.Nivel}<br>
                    <strong>Modalidad:</strong> {solicitud.Modalidad}<br>
                    <strong>Lecciones:</strong> {solicitud.CantidadLecciones}<br>
                    <strong>Requiere equipo:</strong> {(solicitud.RequiereEquipo ? "Sí" : "No")}<br>
                    <strong>A domicilio:</strong> {(solicitud.EsADomicilio ? "Sí" : "No")}<br>
                    <strong>Dirección:</strong> {solicitud.DireccionDomicilio}<br>
                    <strong>Disponibilidad:</strong> {solicitud.Disponibilidad}<br>
                    <strong>Comentarios:</strong> {solicitud.Comentarios}
                    """
                );
            }

            return RedirectToAction(
                nameof(Confirmacion),
                new { id = solicitud.IdSolicitudCurso });
        }

        // GET: /SolicitudesCurso/Confirmacion/5
        [HttpGet]
        public async Task<IActionResult> Confirmacion(int id)
        {
            var idAlumno = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.Disponibilidades)
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == id &&
                    s.IdAlumno == idAlumno);

            if (solicitud == null)
            {
                return NotFound();
            }

            var model = new SolicitudCursoConfirmacionViewModel
            {
                NombreCurso = solicitud.NombreCurso,
                Nivel = solicitud.Nivel,
                Disponibilidad = solicitud.Disponibilidad,
                Comentarios = solicitud.Comentarios,
                FechaSolicitud = solicitud.FechaSolicitud
            };

            return View(
                "~/Views/Perfiles/ConfirmacionSolicitud.cshtml",
                model);
        }
    }
}
