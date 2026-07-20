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
        public async Task<IActionResult> Catalogo()
        {
            await CargarCatalogoAsync();

            return View(
    "~/Views/SolicitudesCurso/CatalogoCursos.cshtml"
);
        }

        // GET: /SolicitudesCurso/Crear
        [HttpGet]
        public async Task<IActionResult> Crear(string? nombreCurso)
        {
            await CargarCatalogoAsync();

            var model = new SolicitudCursoViewModel
            {
                NombreCurso = nombreCurso ?? string.Empty,

                Disponibilidades =
                    new List<DisponibilidadSolicitudViewModel>
                    {
                        new DisponibilidadSolicitudViewModel()
                    }
            };

            return View(
                "~/Views/SolicitudesCurso/SolicitarCurso.cshtml",
                model
            );
        }

        // POST: /SolicitudesCurso/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            SolicitudCursoViewModel model)
        {
            model.Disponibilidades ??=
                new List<DisponibilidadSolicitudViewModel>();

            var tarifa = await _context.TarifasClase
                .Include(t => t.TipoClase)
                .FirstOrDefaultAsync(t =>
                    t.IdTarifaClase == model.IdTarifaClase &&
                    t.Activa &&
                    t.TipoClase.Activo);

            if (tarifa == null)
            {
                ModelState.AddModelError(
                    nameof(model.IdTarifaClase),
                    "La tarifa o paquete seleccionado no está disponible.");
            }

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
                    "Debe indicar al menos una disponibilidad.");
            }
            else
            {
                for (var i = 0;
                     i < model.Disponibilidades.Count;
                     i++)
                {
                    var disponibilidad =
                        model.Disponibilidades[i];

                    if (disponibilidad.HoraHasta <=
                        disponibilidad.HoraDesde)
                    {
                        ModelState.AddModelError(
                            $"Disponibilidades[{i}].HoraHasta",
                            "La hora final debe ser posterior a la hora inicial.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarCatalogoAsync();

                return View(
                    "~/Views/SolicitudesCurso/SolicitarCurso.cshtml",
                    model
                );
            }

            var idAlumno =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return Challenge();
            }

            var resumenDisponibilidad = string.Join(
                "; ",
                model.Disponibilidades.Select(d =>
                    $"{d.DiaSemana}: " +
                    $"{d.HoraDesde:hh\\:mm} - " +
                    $"{d.HoraHasta:hh\\:mm}")
            );

            var solicitud = new SolicitudCurso
            {
                IdAlumno = idAlumno,

                IdCurso = model.IdCurso,

                IdTarifaClase = tarifa!.IdTarifaClase,

                PrecioSolicitado = tarifa.Precio,

                // La oferta seleccionada se conserva en la solicitud.
                NombreCurso = tarifa.Nombre,

                Nivel = model.Nivel,

                Modalidad = tarifa.TipoClase.Nombre,

                CantidadLecciones =
                    tarifa.CantidadLecciones,

                Disponibilidad =
                    resumenDisponibilidad,

                RequiereEquipo =
                    model.RequiereEquipo,

                EsADomicilio =
                    model.EsADomicilio,

                DireccionDomicilio =
                    model.EsADomicilio
                        ? model.DireccionDomicilio?.Trim()
                        : null,

                Comentarios =
                    model.Comentarios?.Trim(),

                Estado = "Pendiente",

                FechaSolicitud = DateTime.Now
            };

            foreach (var disponibilidad
                     in model.Disponibilidades)
            {
                solicitud.Disponibilidades.Add(
                    new DisponibilidadSolicitud
                    {
                        DiaSemana =
                            disponibilidad.DiaSemana,

                        HoraDesde =
                            disponibilidad.HoraDesde,

                        HoraHasta =
                            disponibilidad.HoraHasta
                    });
            }

            _context.SolicitudesCurso.Add(solicitud);

            await _context.SaveChangesAsync();

            await EnviarCorreoSolicitudAsync(
                solicitud,
                tarifa,
                resumenDisponibilidad);

            return RedirectToAction(
                nameof(Confirmacion),
                new
                {
                    id = solicitud.IdSolicitudCurso
                });
        }

        // GET: /SolicitudesCurso/Confirmacion/5
        [HttpGet]
        public async Task<IActionResult> Confirmacion(int id)
        {
            var idAlumno =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var solicitud =
                await _context.SolicitudesCurso
                    .Include(s => s.Disponibilidades)
                    .Include(s => s.TarifaClase)
                        .ThenInclude(t => t.TipoClase)
                    .FirstOrDefaultAsync(s =>
                        s.IdSolicitudCurso == id &&
                        s.IdAlumno == idAlumno);

            if (solicitud == null)
            {
                return NotFound();
            }

            var model =
                new SolicitudCursoConfirmacionViewModel
                {
                    NombreCurso =
                        solicitud.NombreCurso,

                    Nivel =
                        solicitud.Nivel,

                    Disponibilidad =
                        solicitud.Disponibilidad,

                    Comentarios =
                        solicitud.Comentarios,

                    FechaSolicitud =
                        solicitud.FechaSolicitud
                };
            return View(
                "~/Views/SolicitudesCurso/ConfirmacionSolicitud.cshtml",
                model
            );
        }

        private async Task CargarCatalogoAsync()
        {
            ViewBag.Tarifas =
                await _context.TarifasClase
                    .Include(t => t.TipoClase)
                    .Where(t =>
                        t.Activa &&
                        t.TipoClase.Activo)
                    .OrderBy(t => t.IdTarifaClase)
                    .ToListAsync();

            ViewBag.CondicionesServicio =
                await _context.CondicionesServicio
                    .Where(c => c.Activa)
                    .OrderBy(c => c.Orden)
                    .ToListAsync();
        }

        private async Task EnviarCorreoSolicitudAsync(
            SolicitudCurso solicitud,
            TarifaClase tarifa,
            string resumenDisponibilidad)
        {
            var correoDestino =
                _configuration[
                    "AcademiaSettings:CorreoSolicitudes"];

            if (string.IsNullOrWhiteSpace(
                correoDestino))
            {
                return;
            }

            await _emailService.EnviarCorreoAsync(
                correoDestino,
                $"Nueva solicitud SOL-{solicitud.IdSolicitudCurso}",
                $"""
                <strong>Solicitud:</strong> SOL-{solicitud.IdSolicitudCurso}<br>
                <strong>Tarifa o paquete:</strong> {tarifa.Nombre}<br>
                <strong>Tipo de clase:</strong> {tarifa.TipoClase.Nombre}<br>
                <strong>Nivel:</strong> {solicitud.Nivel}<br>
                <strong>Lecciones:</strong> {tarifa.CantidadLecciones}<br>
                <strong>Precio:</strong> ${tarifa.Precio:N2}<br>
                <strong>Requiere equipo:</strong> {(solicitud.RequiereEquipo ? "Sí" : "No")}<br>
                <strong>A domicilio:</strong> {(solicitud.EsADomicilio ? "Sí" : "No")}<br>
                <strong>Dirección:</strong> {solicitud.DireccionDomicilio}<br>
                <strong>Disponibilidad:</strong> {resumenDisponibilidad}<br>
                <strong>Comentarios:</strong> {solicitud.Comentarios}
                """
            );
        }
    }
}
