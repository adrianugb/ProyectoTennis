using AcademiaTennisDAL.Context;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminSolicitudesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public AdminSolicitudesController(
            AppDbContext context,
            EmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
     string? idAlumno,
     string? estado,
     string? tipoClase,
     DateTime? fecha)
        {
            var query = _context.SolicitudesCurso
                .Include(s => s.Alumno)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(idAlumno))
            {
                query = query.Where(s =>
                    s.IdAlumno == idAlumno);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(s =>
                    s.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(tipoClase))
            {
                query = query.Where(s =>
                    s.Modalidad == tipoClase);
            }

            if (fecha.HasValue)
            {
                var fechaInicio = fecha.Value.Date;
                var fechaFin = fechaInicio.AddDays(1);

                query = query.Where(s =>
                    s.FechaSolicitud >= fechaInicio &&
                    s.FechaSolicitud < fechaFin);
            }

            var solicitudes = await query
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var alumnos = await _context.SolicitudesCurso
                .Include(s => s.Alumno)
                .Select(s => new
                {
                    s.IdAlumno,
                    NombreCompleto =
                        s.Alumno.Nombre + " " +
                        s.Alumno.Apellido
                })
                .Distinct()
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();

            ViewBag.Alumnos = new SelectList(
                alumnos,
                "IdAlumno",
                "NombreCompleto",
                idAlumno);

            ViewBag.FiltroIdAlumno =
                idAlumno ?? string.Empty;

            ViewBag.FiltroEstado =
                estado ?? string.Empty;

            ViewBag.FiltroTipoClase =
                tipoClase ?? string.Empty;

            ViewBag.FiltroFecha =
                fecha?.ToString("yyyy-MM-dd")
                ?? string.Empty;

            ViewBag.Estados =
                await _context.SolicitudesCurso
                    .Select(s => s.Estado)
                    .Distinct()
                    .OrderBy(e => e)
                    .ToListAsync();

            ViewBag.TiposClase =
                await _context.SolicitudesCurso
                    .Where(s =>
                        s.Modalidad != null &&
                        s.Modalidad != "")
                    .Select(s => s.Modalidad!)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

            return View(solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.Alumno)
                .Include(s => s.TarifaClase)
                    .ThenInclude(t => t.TipoClase)
                .Include(s => s.Disponibilidades)
                .Include(s => s.ProfesorPropuesto)
                .Include(s => s.CanchaPropuesta)
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revisar(int id)
        {
            var solicitud = await _context.SolicitudesCurso
                .FirstOrDefaultAsync(s => s.IdSolicitudCurso == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado == "Pendiente")
            {
                solicitud.Estado = "En revisión";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                nameof(Proponer),
                new { id = solicitud.IdSolicitudCurso }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Proponer(int id)
        {
            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.Alumno)
                .Include(s => s.Disponibilidades)
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado == "Pendiente")
            {
                solicitud.Estado = "En revisión";
                await _context.SaveChangesAsync();
            }

            await CargarOpcionesPropuestaAsync(
                solicitud.IdProfesorPropuesto,
                solicitud.IdCanchaPropuesta);

            var model = new PropuestaSolicitudViewModel
            {
                IdSolicitudCurso =
                    solicitud.IdSolicitudCurso,

                CodigoSolicitud =
                    $"SOL-{solicitud.IdSolicitudCurso:D4}",

                NombreAlumno =
                    $"{solicitud.Alumno.Nombre} {solicitud.Alumno.Apellido}",

                NombreCurso =
                    solicitud.NombreCurso,

                Nivel =
                    solicitud.Nivel,

                DisponibilidadAlumno =
                    solicitud.Disponibilidad,

                Estado =
                    solicitud.Estado,

                FechaPropuesta =
                    solicitud.FechaPropuesta,

                HoraInicioPropuesta =
                    solicitud.HoraInicioPropuesta,

                HoraFinPropuesta =
                    solicitud.HoraFinPropuesta,

                IdProfesorPropuesto =
                    solicitud.IdProfesorPropuesto,

                IdCanchaPropuesta =
                    solicitud.IdCanchaPropuesta,

                ObservacionesAcademia =
                    solicitud.ObservacionesAcademia
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPropuesta(
           PropuestaSolicitudViewModel model,
           string accion)
        {
            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.Alumno)
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == model.IdSolicitudCurso);

            if (solicitud == null)
            {
                return NotFound();
            }

            var esBorrador = string.Equals(
                accion,
                "borrador",
                StringComparison.OrdinalIgnoreCase);

            var esEnvio = string.Equals(
                accion,
                "enviar",
                StringComparison.OrdinalIgnoreCase);

            if (!esBorrador && !esEnvio)
            {
                return BadRequest();
            }

            if (esBorrador)
            {
                ModelState.Remove(nameof(model.FechaPropuesta));
                ModelState.Remove(nameof(model.HoraInicioPropuesta));
                ModelState.Remove(nameof(model.HoraFinPropuesta));
                ModelState.Remove(nameof(model.IdProfesorPropuesto));
                ModelState.Remove(nameof(model.IdCanchaPropuesta));
            }

            if (model.HoraInicioPropuesta.HasValue &&
                model.HoraFinPropuesta.HasValue &&
                model.HoraFinPropuesta <= model.HoraInicioPropuesta)
            {
                ModelState.AddModelError(
                    nameof(model.HoraFinPropuesta),
                    "La hora de finalización debe ser posterior a la hora de inicio.");
            }

            if (esEnvio &&
                model.FechaPropuesta.HasValue &&
                model.FechaPropuesta.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.FechaPropuesta),
                    "La fecha propuesta no puede ser anterior a la fecha actual.");
            }

            // Validar conflictos de horario antes de enviar la propuesta
            if (esEnvio &&
                model.FechaPropuesta.HasValue &&
                model.HoraInicioPropuesta.HasValue &&
                model.HoraFinPropuesta.HasValue &&
                model.IdProfesorPropuesto.HasValue &&
                model.IdCanchaPropuesta.HasValue)
            {
                var fechaInicio = model.FechaPropuesta.Value.Date;
                var fechaFin = fechaInicio.AddDays(1);

                var horaInicio = model.HoraInicioPropuesta.Value;
                var horaFin = model.HoraFinPropuesta.Value;

                var estadosQueReservanHorario = new[]
                {
        "Propuesta enviada",
        "Aceptada"
    };

                var profesorOcupado =
                    await _context.SolicitudesCurso.AnyAsync(s =>
                        s.IdSolicitudCurso != model.IdSolicitudCurso &&
                        estadosQueReservanHorario.Contains(s.Estado) &&
                        s.FechaPropuesta.HasValue &&
                        s.FechaPropuesta.Value >= fechaInicio &&
                        s.FechaPropuesta.Value < fechaFin &&
                        s.HoraInicioPropuesta.HasValue &&
                        s.HoraFinPropuesta.HasValue &&
                        s.IdProfesorPropuesto ==
                            model.IdProfesorPropuesto.Value &&
                        s.HoraInicioPropuesta.Value < horaFin &&
                        s.HoraFinPropuesta.Value > horaInicio);

                var canchaOcupada =
                    await _context.SolicitudesCurso.AnyAsync(s =>
                        s.IdSolicitudCurso != model.IdSolicitudCurso &&
                        estadosQueReservanHorario.Contains(s.Estado) &&
                        s.FechaPropuesta.HasValue &&
                        s.FechaPropuesta.Value >= fechaInicio &&
                        s.FechaPropuesta.Value < fechaFin &&
                        s.HoraInicioPropuesta.HasValue &&
                        s.HoraFinPropuesta.HasValue &&
                        s.IdCanchaPropuesta ==
                            model.IdCanchaPropuesta.Value &&
                        s.HoraInicioPropuesta.Value < horaFin &&
                        s.HoraFinPropuesta.Value > horaInicio);

                if (profesorOcupado)
                {
                    ModelState.AddModelError(
                        nameof(model.IdProfesorPropuesto),
                        "El profesor seleccionado ya tiene una clase programada en ese horario.");
                }

                if (canchaOcupada)
                {
                    ModelState.AddModelError(
                        nameof(model.IdCanchaPropuesta),
                        "La cancha seleccionada ya está reservada en ese horario.");
                }
            }


            if (!ModelState.IsValid)
            {
                await CargarOpcionesPropuestaAsync(
                    model.IdProfesorPropuesto,
                    model.IdCanchaPropuesta);

                model.CodigoSolicitud =
                    $"SOL-{solicitud.IdSolicitudCurso:D4}";

                model.NombreAlumno =
                    $"{solicitud.Alumno.Nombre} {solicitud.Alumno.Apellido}";

                model.NombreCurso =
                    solicitud.NombreCurso;

                model.Nivel =
                    solicitud.Nivel;

                model.DisponibilidadAlumno =
                    solicitud.Disponibilidad;

                model.Estado =
                    solicitud.Estado;

                return View("Proponer", model);
            }

            solicitud.FechaPropuesta =
                model.FechaPropuesta;

            solicitud.HoraInicioPropuesta =
                model.HoraInicioPropuesta;

            solicitud.HoraFinPropuesta =
                model.HoraFinPropuesta;

            solicitud.IdProfesorPropuesto =
                model.IdProfesorPropuesto;

            solicitud.IdCanchaPropuesta =
                model.IdCanchaPropuesta;

            solicitud.ObservacionesAcademia =
                model.ObservacionesAcademia?.Trim();

            if (esBorrador)
            {
                solicitud.Estado = "En revisión";

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "El borrador de la propuesta fue guardado correctamente.";

                return RedirectToAction(
                    nameof(Proponer),
                    new
                    {
                        id = solicitud.IdSolicitudCurso
                    });
            }

            solicitud.Estado = "Propuesta enviada";

            await _context.SaveChangesAsync();

            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p =>
                    p.Id == solicitud.IdProfesorPropuesto);

            var cancha = await _context.Canchas
                .FirstOrDefaultAsync(c =>
                    c.IdCancha == solicitud.IdCanchaPropuesta);

            var nombreProfesor =
                profesor == null
                    ? "Por confirmar"
                    : $"{profesor.Nombre} {profesor.Apellidos}";

            var nombreCancha =
                cancha?.Nombre ?? "Por confirmar";

            var codigoSolicitud =
                $"SOL-{solicitud.IdSolicitudCurso:D4}";

            var baseUrl =
            _configuration["AcademiaSettings:BaseUrl"];

            var urlRespuesta =
                string.IsNullOrWhiteSpace(baseUrl)
                    ? "#"
                    : $"{baseUrl.TrimEnd('/')}/SolicitudesCurso/ResponderPropuesta/{solicitud.IdSolicitudCurso}";

            if (!string.IsNullOrWhiteSpace(solicitud.Alumno.Email))
            {
                try
                {
                    var html = ConstruirCorreoPropuesta(
                        solicitud,
                        codigoSolicitud,
                        nombreProfesor,
                        nombreCancha,
                        urlRespuesta);

                    await _emailService.EnviarCorreoAsync(
                        solicitud.Alumno.Email,
                        $"Propuesta de horario - {codigoSolicitud}",
                        html);
                }
                catch (Exception)
                {
                    TempData["AdvertenciaCorreo"] =
                        "La propuesta fue guardada, pero no se pudo enviar el correo al alumno.";
                }
            }
            else
            {
                TempData["AdvertenciaCorreo"] =
                    "La propuesta fue guardada, pero el alumno no tiene correo registrado.";
            }

            var whatsapp = ConstruirWhatsappPropuesta(
                solicitud,
                codigoSolicitud,
                nombreProfesor,
                nombreCancha);

            if (!string.IsNullOrWhiteSpace(whatsapp))
            {
                TempData["WhatsappUrl"] = whatsapp;
            }
            else
            {
                TempData["AdvertenciaWhatsapp"] =
                    "El alumno no tiene un número de teléfono registrado.";
            }

            TempData["Success"] =
                "La propuesta fue enviada correctamente.";

            return RedirectToAction(
                nameof(Detalle),
                new
                {
                    id = solicitud.IdSolicitudCurso
                });
        }


        private static string ConstruirCorreoPropuesta(
    AcademiaTennisDAL.Entities.SolicitudCurso solicitud,
     string codigoSolicitud,
    string nombreProfesor,
    string nombreCancha,
    string urlRespuesta)
        {
            var fecha =
                solicitud.FechaPropuesta?
                    .ToString("dd/MM/yyyy")
                ?? "Por confirmar";

            var horaInicio =
                solicitud.HoraInicioPropuesta?
                    .ToString(@"hh\:mm")
                ?? "Por confirmar";

            var horaFin =
                solicitud.HoraFinPropuesta?
                    .ToString(@"hh\:mm")
                ?? "Por confirmar";

            var observaciones =
                string.IsNullOrWhiteSpace(
                    solicitud.ObservacionesAcademia)
                    ? "Sin observaciones adicionales."
                    : solicitud.ObservacionesAcademia;

            return $"""
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
</head>

<body style="margin:0; padding:0; background:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#222222;">

    <table role="presentation"
           width="100%"
           cellspacing="0"
           cellpadding="0"
           style="background:#f4f6f8; padding:30px 15px;">

        <tr>
            <td align="center">

                <table role="presentation"
                       width="100%"
                       cellspacing="0"
                       cellpadding="0"
                       style="max-width:650px; background:#ffffff; border-radius:14px; overflow:hidden;">

                    <tr>
                        <td style="background:#95c11f; padding:24px 30px; color:#ffffff;">

                            <h1 style="margin:0; font-size:24px;">
                                Propuesta de horario
                            </h1>

                            <p style="margin:8px 0 0; font-size:14px;">
                                La academia preparó una propuesta para tu solicitud.
                            </p>

                        </td>
                    </tr>

                    <tr>
                        <td style="padding:30px;">

                            <p style="margin-top:0; font-size:15px; line-height:1.6;">
                                Hola, <strong>{solicitud.Alumno.Nombre}</strong>.
                            </p>

                            <p style="font-size:15px; line-height:1.6;">
                                Revisamos tu disponibilidad y preparamos la siguiente
                                propuesta de clase:
                            </p>

                            <table role="presentation"
                                   width="100%"
                                   cellspacing="0"
                                   cellpadding="0"
                                   style="margin-top:24px; border-collapse:collapse;">

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; width:38%; font-weight:bold;">
                                        Código de solicitud
                                    </td>

                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {codigoSolicitud}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Clase o paquete
                                    </td>

                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {solicitud.NombreCurso}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Fecha
                                    </td>

                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {fecha}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Horario
                                    </td>

                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {horaInicio} - {horaFin}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Profesor
                                    </td>

                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {nombreProfesor}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Cancha
                                    </td>

                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {nombreCancha}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Observaciones
                                    </td>

                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {observaciones}
                                    </td>
                                </tr>

                            </table>

  <table role="presentation"
       align="center"
       cellspacing="0"
       cellpadding="0"
       border="0"
       style="margin:30px auto;">
    <tr>
        <td align="center"
            bgcolor="#95c11f"
            style="background-color:#95c11f; border-radius:8px;">

            <a href="{urlRespuesta}"
               target="_blank"
               style="display:inline-block;
                      background-color:#95c11f;
                      border:1px solid #95c11f;
                      border-radius:8px;
                      padding:14px 24px;
                      font-family:Arial, Helvetica, sans-serif;
                      font-size:15px;
                      font-weight:bold;
                      line-height:20px;
                      color:#ffffff !important;
                      text-decoration:none;">

                Ver y responder propuesta

            </a>

        </td>
    </tr>
</table>


<p style="margin:28px 0 0; font-size:13px; color:#666666; line-height:1.5;">
    También puedes iniciar sesión en el sistema y buscar la solicitud
    <strong>{codigoSolicitud}</strong>.
</p>

                        </td>
                    </tr>

                    <tr>
                        <td style="background:#f7f8f9; padding:18px 30px; text-align:center; font-size:12px; color:#777777;">
                            Academia de Tennis<br>
                            Notificación automática del sistema
                        </td>
                    </tr>

                </table>

            </td>
        </tr>

    </table>

</body>
</html>
""";
        }

        private static string? ConstruirWhatsappPropuesta(
    AcademiaTennisDAL.Entities.SolicitudCurso solicitud,
    string codigoSolicitud,
    string nombreProfesor,
    string nombreCancha)
        {
            var telefonoAlumno =
                solicitud.Alumno.PhoneNumber;

            if (string.IsNullOrWhiteSpace(telefonoAlumno))
            {
                return null;
            }

            var telefonoLimpio =
                new string(
                    telefonoAlumno
                        .Where(char.IsDigit)
                        .ToArray());

            if (string.IsNullOrWhiteSpace(telefonoLimpio))
            {
                return null;
            }

            var fecha =
                solicitud.FechaPropuesta?
                    .ToString("dd/MM/yyyy")
                ?? "Por confirmar";

            var horaInicio =
                solicitud.HoraInicioPropuesta?
                    .ToString(@"hh\:mm")
                ?? "Por confirmar";

            var horaFin =
                solicitud.HoraFinPropuesta?
                    .ToString(@"hh\:mm")
                ?? "Por confirmar";

            var mensajeWhatsapp =
                $"*Academia de Tennis*\n\n" +
                $"*Propuesta de horario*\n\n" +
                $"Hola, {solicitud.Alumno.Nombre}.\n\n" +
                $"Tenemos una propuesta para tu solicitud.\n\n" +
                $"*Código de solicitud:* {codigoSolicitud}\n" +
                $"*Clase o paquete:* {solicitud.NombreCurso}\n" +
                $"*Fecha:* {fecha}\n" +
                $"*Horario:* {horaInicio} - {horaFin}\n" +
                $"*Profesor:* {nombreProfesor}\n" +
                $"*Cancha:* {nombreCancha}";

            if (!string.IsNullOrWhiteSpace(
                    solicitud.ObservacionesAcademia))
            {
                mensajeWhatsapp +=
                    $"\n*Observaciones:* " +
                    $"{solicitud.ObservacionesAcademia}";
            }

            mensajeWhatsapp +=
                "\n\nIngresa al sistema para aceptar o rechazar la propuesta.";

            return $"https://wa.me/{telefonoLimpio}" +
                   $"?text={Uri.EscapeDataString(mensajeWhatsapp)}";
        }


        private async Task CargarOpcionesPropuestaAsync(
           int? idProfesorSeleccionado = null,
           int? idCanchaSeleccionada = null)
        {
            var profesores = await _context.Profesores
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.Apellidos)
                .ToListAsync();

            var canchas = await _context.Canchas
                .Where(c =>
                    c.Disponible &&
                    !c.EnMantenimiento)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Profesores = new SelectList(
                profesores.Select(p => new
                {
                    IdProfesor = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellidos}"
                }),
                "IdProfesor",
                "NombreCompleto",
                idProfesorSeleccionado
            );

            ViewBag.Canchas = new SelectList(
                canchas,
                "IdCancha",
                "Nombre",
                idCanchaSeleccionada
            );
        }
    }



}