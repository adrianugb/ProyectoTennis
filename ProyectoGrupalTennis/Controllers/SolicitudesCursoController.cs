using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class SolicitudesCursoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public SolicitudesCursoController(
        AppDbContext context,
        EmailService emailService,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _userManager = userManager;
        }


        // GET: /SolicitudesCurso/Catalogo
        [HttpGet]
        public async Task<IActionResult> Catalogo(
            string? buscar,
            int? idTipoClase)
        {
            var query = _context.TarifasClase
                .Include(t => t.TipoClase)
                .Where(t =>
                    t.Activa &&
                    t.TipoClase.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                query = query.Where(t =>
                    t.Nombre.Contains(buscar) ||
                    t.Descripcion.Contains(buscar) ||
                    t.TipoClase.Nombre.Contains(buscar));
            }

            if (idTipoClase.HasValue)
            {
                query = query.Where(t =>
                    t.IdTipoClase == idTipoClase.Value);
            }

            ViewBag.Tarifas = await query
                .OrderBy(t => t.TipoClase.Nombre)
                .ThenBy(t => t.CantidadLecciones)
                .ThenBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.TiposClase = await _context.TiposClase
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.CondicionesServicio =
                await _context.CondicionesServicio
                    .Where(c => c.Activa)
                    .OrderBy(c => c.Orden)
                    .ToListAsync();

            ViewBag.FiltroBuscar = buscar;
            ViewBag.FiltroTipoClase = idTipoClase;

            return View(
                "~/Views/SolicitudesCurso/CatalogoCursos.cshtml"
            );
        }

        // GET: /SolicitudesCurso/Crear
        [HttpGet]
        public async Task<IActionResult> Crear(int? idTarifaClase)
        {
            if (!idTarifaClase.HasValue)
            {
                TempData["Error"] =
                    "Debe seleccionar una tarifa o paquete.";

                return RedirectToAction(nameof(Catalogo));
            }

            var tarifa = await _context.TarifasClase
                .Include(t => t.TipoClase)
                .FirstOrDefaultAsync(t =>
                    t.IdTarifaClase == idTarifaClase.Value &&
                    t.Activa &&
                    t.TipoClase.Activo);

            if (tarifa == null)
            {
                TempData["Error"] =
                    "La tarifa seleccionada no está disponible.";

                return RedirectToAction(nameof(Catalogo));
            }

            await CargarCatalogoAsync();

            var model = new SolicitudCursoViewModel
            {
                IdTarifaClase = tarifa.IdTarifaClase,
                NombreCurso = tarifa.Nombre,
                TipoClase = tarifa.TipoClase.Nombre,
                CondicionMatricula = tarifa.CondicionMatricula,
                CantidadLecciones = tarifa.CantidadLecciones,
                PrecioPorPersona = tarifa.PrecioPorPersona,
                PrecioEstimado = tarifa.Precio,

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

            foreach (var disponibilidad in model.Disponibilidades)
            {
                solicitud.Disponibilidades.Add(
                    new DisponibilidadSolicitud
                    {
                        DiaSemana = disponibilidad.DiaSemana,
                        HoraDesde = disponibilidad.HoraDesde,
                        HoraHasta = disponibilidad.HoraHasta
                    });
            }

            // 1. Agregar la solicitud al contexto
            _context.SolicitudesCurso.Add(solicitud);

            // 2. Guardar para que MySQL genere IdSolicitudCurso
            await _context.SaveChangesAsync();

            // 3. Intentar enviar correo sin romper el flujo
            try
            {
                await EnviarCorreoSolicitudAsync(
                    solicitud,
                    tarifa,
                    resumenDisponibilidad);
            }
            catch (Exception)
            {
                TempData["AdvertenciaCorreo"] =
                    "La solicitud fue guardada correctamente, " +
                    "pero no se pudo enviar la notificación por correo.";
            }

            // 4. Redirigir usando el ID ya generado
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

            var numeroWhatsapp =
        _configuration["AcademiaSettings:WhatsappSolicitudes"];

            var identificador =
                $"SOL-{solicitud.IdSolicitudCurso:D4}";

            var mensajeWhatsapp =
                  $"🎾 *Academia de Tennis*\n\n" +
                  $"*Nueva solicitud de clase*\n\n" +
                  $"Hola, deseo dar seguimiento a mi solicitud.\n\n" +
                  $"*Código de solicitud:* {identificador}\n" +
                  $"*Clase o paquete:* {solicitud.NombreCurso}\n" +
                  $"*Nivel:* {solicitud.Nivel}\n" +
                  $"*Disponibilidad:* {solicitud.Disponibilidad}";

            if (!string.IsNullOrWhiteSpace(solicitud.Comentarios))
            {
                mensajeWhatsapp +=
                    $"\n*Comentarios:* {solicitud.Comentarios}";
            }

            mensajeWhatsapp +=
                "\n\nGracias. Quedo atento(a) a la confirmación.";

            var whatsappUrl =
                string.IsNullOrWhiteSpace(numeroWhatsapp)
                    ? null
                    : $"https://wa.me/{numeroWhatsapp}" +
                      $"?text={Uri.EscapeDataString(mensajeWhatsapp)}";

            var model =
                new SolicitudCursoConfirmacionViewModel
                {
                    IdSolicitudCurso =
                        solicitud.IdSolicitudCurso,

                    NombreCurso =
                        solicitud.NombreCurso,

                    Nivel =
                        solicitud.Nivel,

                    Disponibilidad =
                        solicitud.Disponibilidad,

                    Comentarios =
                        solicitud.Comentarios,

                    FechaSolicitud =
                        solicitud.FechaSolicitud,

                    WhatsappUrl =
                        whatsappUrl
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
                _configuration["AcademiaSettings:CorreoSolicitudes"];

            if (string.IsNullOrWhiteSpace(correoDestino))
            {
                throw new InvalidOperationException(
                    "No está configurado el correo de solicitudes de la academia.");
            }

            var baseUrl =
     _configuration["AcademiaSettings:BaseUrl"];

            var urlGestion =
         string.IsNullOrWhiteSpace(baseUrl)
             ? "#"
             : $"{baseUrl.TrimEnd('/')}/AdminSolicitudes/Detalle/{solicitud.IdSolicitudCurso}";

            var cuerpoCorreo = $"""
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
</head>
<body style="margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#222222;">

    <table role="presentation"
           width="100%"
           cellspacing="0"
           cellpadding="0"
           style="background-color:#f4f6f8; padding:30px 15px;">

        <tr>
            <td align="center">

                <table role="presentation"
                       width="100%"
                       cellspacing="0"
                       cellpadding="0"
                       style="max-width:650px; background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 8px 24px rgba(0,0,0,0.08);">

                    <tr>
                        <td style="background:#95c11f; padding:24px 30px; color:#ffffff;">
                            <h1 style="margin:0; font-size:24px;">
                                Nueva solicitud de clase
                            </h1>

                            <p style="margin:8px 0 0; font-size:14px;">
                                Se registró una nueva solicitud en el sistema.
                            </p>
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:30px;">

                            <p style="margin-top:0; font-size:15px; line-height:1.6;">
                                Hola,
                            </p>

                            <p style="font-size:15px; line-height:1.6;">
                                Un alumno registró una nueva solicitud de clase.
                                Revisa la información y continúa con la asignación de
                                horario, profesor y cancha.
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
                                        SOL-{solicitud.IdSolicitudCurso:D4}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Clase o paquete
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {tarifa.Nombre}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Tipo de clase
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {tarifa.TipoClase.Nombre}
                                    </td>
                                </tr>

                                   <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Cantidad de sesiones
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                         {tarifa.CantidadLecciones}
                                        {(tarifa.CantidadLecciones == 1 ? " sesión" : " sesiones")}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Precio
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                       ${tarifa.Precio:N2}
                                        {(tarifa.PrecioPorPersona
                                            ? " por persona"
                                            : tarifa.CantidadLecciones == 1
                                                ? " por sesión"
                                                : " precio total")}
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Nivel
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {solicitud.Nivel}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Disponibilidad
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {resumenDisponibilidad}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Requiere equipo
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {(solicitud.RequiereEquipo ? "Sí" : "No")}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Clase a domicilio
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {(solicitud.EsADomicilio ? "Sí" : "No")}
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee; font-weight:bold;">
                                        Comentarios
                                    </td>
                                    <td style="padding:10px 0; border-bottom:1px solid #eeeeee;">
                                        {(string.IsNullOrWhiteSpace(solicitud.Comentarios)
                                                        ? "Sin comentarios"
                                                        : solicitud.Comentarios)}
                                    </td>
                                </tr>

                            </table>


                            <div style="text-align:center;margin-top:35px;">

                                <a href="{urlGestion}"
                                   style="background:#95c11f;
                                          color:#ffffff;
                                          padding:14px 24px;
                                          text-decoration:none;
                                          border-radius:8px;
                                          font-weight:bold;
                                          display:inline-block;">

                                    Revisar solicitud

                                </a>

                            </div>

                           <p style="margin:28px 0 0; font-size:13px; color:#666666; line-height:1.5;">
                            Inicia sesión en el sistema para revisar la solicitud,
                            asignar un profesor, definir el horario y contactar al alumno.

                            <br><br>

                            Código de referencia:
                            <strong>SOL-{solicitud.IdSolicitudCurso:D4}</strong>
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

            await _emailService.EnviarCorreoAsync(
                correoDestino,
                $"Nueva solicitud SOL-{solicitud.IdSolicitudCurso:D4}",
                cuerpoCorreo
            );
        }


        private async Task EnviarCorreoRechazoAsync(
           SolicitudCurso solicitud)
        {

   
            var correoDestino =
                _configuration["AcademiaSettings:CorreoSolicitudes"];

            if (string.IsNullOrWhiteSpace(correoDestino))
            {
                return;
            }

            var baseUrl =
                _configuration["AcademiaSettings:BaseUrl"];
            var urlGestion =
                string.IsNullOrWhiteSpace(baseUrl)
                    ? "#"
                    : $"{baseUrl.TrimEnd('/')}/AdminSolicitudes/Detalle/{solicitud.IdSolicitudCurso}";

            var fechaPropuesta =
                solicitud.FechaPropuesta?.ToString("dd/MM/yyyy")
                ?? "Sin fecha definida";

            var horaInicio =
                solicitud.HoraInicioPropuesta?.ToString(@"hh\:mm")
                ?? "Sin hora";

            var horaFin =
                solicitud.HoraFinPropuesta?.ToString(@"hh\:mm")
                ?? "Sin hora";

            var motivoRechazo =
                string.IsNullOrWhiteSpace(solicitud.MotivoRechazoAlumno)
                    ? "Sin motivo indicado."
                    : solicitud.MotivoRechazoAlumno;

            var cuerpoCorreo = $"""

<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="UTF-8">
</head>

<body style="margin:0;padding:0;background:#f4f6f8;font-family:Arial,Helvetica,sans-serif;">

<table width="100%" cellpadding="0" cellspacing="0" style="padding:30px;">

<tr>
<td align="center">

<table width="650"
       cellpadding="0"
       cellspacing="0"
       style="background:#ffffff;border-radius:14px;overflow:hidden;">

<tr>
<td style="background:#f39c12;padding:24px;color:white;">

<h2 style="margin:0;">
Propuesta rechazada por el alumno
</h2>

<p style="margin-top:8px;">
La solicitud requiere una nueva revisión.
</p>

</td>
</tr>

<tr>
<td style="padding:30px;">

<table width="100%" style="border-collapse:collapse;">

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Código
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
SOL-{solicitud.IdSolicitudCurso:D4}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Servicio solicitado
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
{solicitud.NombreCurso}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Fecha propuesta
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
{fechaPropuesta}</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Horario
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
{horaInicio} - {horaFin}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;vertical-align:top;">
Motivo del alumno
</td>

<td style="padding:10px;">
{motivoRechazo}</td>
</tr>

</table>

<div style="text-align:center;margin-top:35px;">

<a href="{urlGestion}"
style="background:#95c11f;
color:white;
padding:14px 24px;
text-decoration:none;
border-radius:8px;
font-weight:bold;">

Revisar solicitud

</a>

</div>

</td>
</tr>

<tr>

<td style="background:#f7f8f9;
padding:18px;
text-align:center;
font-size:12px;
color:#777;">

Academia de Tennis<br/>
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


            await _emailService.EnviarCorreoAsync(
                correoDestino,
                $"Propuesta rechazada - SOL-{solicitud.IdSolicitudCurso:D4}",
                cuerpoCorreo);


        }


        private async Task EnviarCorreoAceptacionAsync(
    SolicitudCurso solicitud)
        {
            var correoDestino =
                _configuration["AcademiaSettings:CorreoSolicitudes"];

            if (string.IsNullOrWhiteSpace(correoDestino))
            {
                return;
            }

            var baseUrl =
                _configuration["AcademiaSettings:BaseUrl"];

            var urlGestion =
                string.IsNullOrWhiteSpace(baseUrl)
                    ? "#"
                    : $"{baseUrl.TrimEnd('/')}/AdminSolicitudes/Detalle/{solicitud.IdSolicitudCurso}";

            var fechaPropuesta =
                solicitud.FechaPropuesta?.ToString("dd/MM/yyyy")
                ?? "Sin fecha definida";

            var horaInicio =
                solicitud.HoraInicioPropuesta?.ToString(@"hh\:mm")
                ?? "Sin hora";

            var horaFin =
                solicitud.HoraFinPropuesta?.ToString(@"hh\:mm")
                ?? "Sin hora";

            var profesor =
                solicitud.ProfesorPropuesto == null
                    ? "Por confirmar"
                    : $"{solicitud.ProfesorPropuesto.Nombre} {solicitud.ProfesorPropuesto.Apellidos}";

            var cancha =
                solicitud.CanchaPropuesta?.Nombre
                ?? "Por confirmar";

            var cuerpoCorreo = $"""
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
</head>

<body style="margin:0;padding:0;background:#f4f6f8;font-family:Arial,Helvetica,sans-serif;">

<table width="100%" cellpadding="0" cellspacing="0" style="padding:30px;">
<tr>
<td align="center">

<table width="650"
       cellpadding="0"
       cellspacing="0"
       style="background:#ffffff;border-radius:14px;overflow:hidden;">

<tr>
<td style="background:#95c11f;padding:24px;color:white;">

<h2 style="margin:0;">
Propuesta aceptada por el alumno
</h2>

<p style="margin-top:8px;">
El alumno confirmó el horario desde el sistema.
</p>

</td>
</tr>

<tr>
<td style="padding:30px;">

<table width="100%" style="border-collapse:collapse;">

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Código
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
SOL-{solicitud.IdSolicitudCurso:D4}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Servicio
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
{solicitud.NombreCurso}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Fecha aceptada
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
{fechaPropuesta}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Horario
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
{horaInicio} - {horaFin}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;border-bottom:1px solid #eee;">
Profesor
</td>

<td style="padding:10px;border-bottom:1px solid #eee;">
{profesor}
</td>
</tr>

<tr>
<td style="padding:10px;font-weight:bold;">
Cancha
</td>

<td style="padding:10px;">
{cancha}
</td>
</tr>

</table>

<div style="text-align:center;margin-top:35px;">

<a href="{urlGestion}"
   style="background:#95c11f;
          color:white;
          padding:14px 24px;
          text-decoration:none;
          border-radius:8px;
          font-weight:bold;">

Ver solicitud

</a>

</div>

</td>
</tr>

<tr>
<td style="background:#f7f8f9;
           padding:18px;
           text-align:center;
           font-size:12px;
           color:#777;">

Academia de Tennis<br/>
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

            await _emailService.EnviarCorreoAsync(
                correoDestino,
                $"Propuesta aceptada - SOL-{solicitud.IdSolicitudCurso:D4}",
                cuerpoCorreo);
        }

        private string? ConstruirWhatsappRespuestaAlumno(
    SolicitudCurso solicitud,
    bool aceptada)
        {
            var telefonoAcademia =
                _configuration["AcademiaSettings:WhatsappSolicitudes"];

            if (string.IsNullOrWhiteSpace(telefonoAcademia))
            {
                return null;
            }

            var telefonoLimpio =
                new string(
                    telefonoAcademia
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

            var codigoSolicitud =
                $"SOL-{solicitud.IdSolicitudCurso:D4}";

            string mensaje;

            if (aceptada)
            {
                mensaje =
                    $"Hola. Confirmo que acepté la propuesta de horario para la solicitud {codigoSolicitud} " +
                    $"y la acepté desde el sistema.\n\n" +
                    $"*Detalles de la propuesta aceptada:*\n" +
                    $"*Servicio:* {solicitud.NombreCurso}\n" +
                    $"*Fecha:* {fecha}\n" +
                    $"*Horario:* {horaInicio} - {horaFin}\n" +
                    $"*Profesor:* " +
                    $"{solicitud.ProfesorPropuesto?.Nombre} " +
                    $"{solicitud.ProfesorPropuesto?.Apellidos}\n" +
                    $"*Cancha:* {solicitud.CanchaPropuesta?.Nombre}\n\n" +
                    $"Quedo atento(a) a cualquier información adicional.";
            }
            else
            {
                mensaje =
                    $"Hola. He revisado la propuesta de horario de la solicitud {codigoSolicitud} " +
                    $"y no puedo aceptarla.\n\n" +
                    $"*Propuesta recibida:*\n" +
                    $"*Servicio:* {solicitud.NombreCurso}\n" +
                    $"*Fecha:* {fecha}\n" +
                    $"*Horario:* {horaInicio} - {horaFin}\n\n" +
                    $"*Comentario o nueva disponibilidad:*\n" +
                    $"{solicitud.MotivoRechazoAlumno}\n\n" +
                    $"Quedo pendiente de una nueva propuesta.";
            }

            return $"https://wa.me/{telefonoLimpio}" +
                   $"?text={Uri.EscapeDataString(mensaje)}";
        }


        private async Task CrearNotificacionAdministradoresAsync(
        SolicitudCurso solicitud,
        bool aceptada)
        {
            var administradores =
                await _userManager.GetUsersInRoleAsync("Administrador");

            var codigoSolicitud =
                $"SOL-{solicitud.IdSolicitudCurso:D4}";

            var titulo = aceptada
                ? $"Propuesta aceptada - {codigoSolicitud}"
                : $"Propuesta rechazada - {codigoSolicitud}";

            var mensaje = aceptada
                ? $"El alumno aceptó la propuesta de horario para la solicitud " +
                  $"{codigoSolicitud}. Fecha: " +
                  $"{solicitud.FechaPropuesta:dd/MM/yyyy}, horario: " +
                  $"{solicitud.HoraInicioPropuesta:hh\\:mm} - " +
                  $"{solicitud.HoraFinPropuesta:hh\\:mm}."
                : $"El alumno rechazó la propuesta de horario para la solicitud " +
                  $"{codigoSolicitud}. Motivo o nueva disponibilidad: " +
                  $"{solicitud.MotivoRechazoAlumno}";

            foreach (var administrador in administradores)
            {
                var yaExiste = await _context.Notificaciones.AnyAsync(n =>
                    n.IdUsuario == administrador.Id &&
                    n.Tipo == (aceptada
                        ? "Propuesta aceptada"
                        : "Propuesta rechazada") &&
                    n.Titulo == titulo &&
                    n.FechaEnvio >= DateTime.Now.AddMinutes(-2));

                if (yaExiste)
                {
                    continue;
                }

                _context.Notificaciones.Add(
                    new Notificacion
                    {
                        IdUsuario = administrador.Id,
                        Tipo = aceptada
                            ? "Propuesta aceptada"
                            : "Propuesta rechazada",
                        Titulo = titulo,
                        Mensaje = mensaje,
                        Leida = false,
                        FechaEnvio = DateTime.Now,
                        CanalUsado = "Plataforma",
                        EnvioFallido = false
                    });
            }
        }


        [HttpGet]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ResponderPropuesta(int id)
        {
            var idAlumno = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.ProfesorPropuesto)
                .Include(s => s.CanchaPropuesta)
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == id &&
                    s.IdAlumno == idAlumno);

            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        [HttpGet]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> MisSolicitudes()
        {
            var idAlumno = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var solicitudes = await _context.SolicitudesCurso
                .Include(s => s.ProfesorPropuesto)
                .Include(s => s.CanchaPropuesta)
                .Where(s => s.IdAlumno == idAlumno)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        [HttpPost]
        [Authorize(Roles = "Usuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AceptarPropuesta(int id)
        {
            var idAlumno = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var solicitud = await _context.SolicitudesCurso
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == id &&
                    s.IdAlumno == idAlumno);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Propuesta enviada")
            {
                TempData["Error"] =
                    "Esta propuesta ya fue respondida o no está disponible.";

                return RedirectToAction(
                    nameof(ResponderPropuesta),
                    new { id });
            }

            return RedirectToAction(
                nameof(ConfirmarPagoSolicitud),
                new { id });
        }

        [HttpGet]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConfirmarPagoSolicitud(int id)
        {
            var idAlumno = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return Challenge();
            }

            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.ProfesorPropuesto)
                .Include(s => s.CanchaPropuesta)
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == id &&
                    s.IdAlumno == idAlumno);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Propuesta enviada")
            {
                TempData["Error"] =
                    "Esta propuesta ya fue respondida o no está disponible.";

                return RedirectToAction(
                    nameof(ResponderPropuesta),
                    new { id });
            }

            if (!solicitud.PrecioSolicitado.HasValue ||
                solicitud.PrecioSolicitado.Value <= 0)
            {
                TempData["Error"] =
                    "No se encontró un monto válido para esta solicitud.";

                return RedirectToAction(
                    nameof(ResponderPropuesta),
                    new { id });
            }

            UbicacionAlumno? ubicacion = null;

            decimal distanciaKm = 0;
            decimal costoFijo = 0;
            decimal tarifaPorKm = 0;
            decimal costoPorDistancia = 0;
            decimal costoDesplazamiento = 0;

            if (solicitud.EsADomicilio)
            {
                ubicacion = await _context.UbicacionesAlumno
                    .Include(u => u.Zona)
                    .FirstOrDefaultAsync(u =>
                        u.IdAlumno == idAlumno &&
                        u.EsPrincipal);

                if (ubicacion == null)
                {
                    TempData["Error"] =
                        "Debe registrar una ubicación principal para calcular el costo de la clase a domicilio.";

                    return RedirectToAction(
                        "MiUbicacion",
                        "Geolocalizacion");
                }

                if (ubicacion.Zona == null ||
                    !ubicacion.Zona.Activa ||
                    !ubicacion.Zona.LatitudCentro.HasValue ||
                    !ubicacion.Zona.LongitudCentro.HasValue)
                {
                    TempData["Error"] =
                        "La ubicación registrada no tiene una zona de cobertura válida.";

                    return RedirectToAction(
                        nameof(ResponderPropuesta),
                        new { id });
                }

                double distanciaCalculada = CalcularDistanciaKm(
                    (double)ubicacion.Latitud,
                    (double)ubicacion.Longitud,
                    (double)ubicacion.Zona.LatitudCentro.Value,
                    (double)ubicacion.Zona.LongitudCentro.Value);

                if (ubicacion.Zona.RadioMaximoKm.HasValue &&
                    distanciaCalculada >
                    (double)ubicacion.Zona.RadioMaximoKm.Value)
                {
                    TempData["Error"] =
                        "La ubicación está fuera del radio permitido para clases a domicilio.";

                    return RedirectToAction(
                        nameof(ResponderPropuesta),
                        new { id });
                }

                distanciaKm = Math.Round(
                    Convert.ToDecimal(distanciaCalculada),
                    2);

                costoFijo =
                    ubicacion.Zona.CostoAdicional;

                tarifaPorKm =
                    ubicacion.Zona.TarifaPorKm ?? 0;

                costoPorDistancia = Math.Round(
                    distanciaKm * tarifaPorKm,
                    2);

                costoDesplazamiento = Math.Round(
                    costoFijo + costoPorDistancia,
                    2);
            }

            decimal montoBase =
                solicitud.PrecioSolicitado.Value;

            decimal montoTotal =
                montoBase + costoDesplazamiento;

            var model = new ConfirmarPagoSolicitudViewModel
            {
                IdSolicitudCurso =
                    solicitud.IdSolicitudCurso,

                Concepto =
                    solicitud.NombreCurso,

                Monto =
                    montoBase,

                MontoTotal =
                    montoTotal,

                EsADomicilio =
                    solicitud.EsADomicilio,

                DireccionDomicilio =
                    solicitud.DireccionDomicilio,

                FechaPropuesta =
                    solicitud.FechaPropuesta,

                HoraInicioPropuesta =
                    solicitud.HoraInicioPropuesta,

                HoraFinPropuesta =
                    solicitud.HoraFinPropuesta,

                Profesor =
                    solicitud.ProfesorPropuesto == null
                        ? "Por confirmar"
                        : $"{solicitud.ProfesorPropuesto.Nombre} " +
                          $"{solicitud.ProfesorPropuesto.Apellidos}",

                Cancha =
                    solicitud.CanchaPropuesta?.Nombre,

                TieneUbicacion =
                    ubicacion != null,

                NombreZona =
                    ubicacion?.Zona?.Nombre,

                DireccionUbicacion =
                    ubicacion?.DireccionCompleta,

                DistanciaKm =
                    distanciaKm,

                CostoFijoZona =
                    costoFijo,

                TarifaPorKm =
                    tarifaPorKm,

                CostoPorDistancia =
                    costoPorDistancia,

                CostoDesplazamiento =
                    costoDesplazamiento
            };

            return View(
                "~/Views/Pagos/ConfirmarPagoSolicitud.cshtml",
                model);
        }

        [HttpPost]
        [Authorize(Roles = "Usuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPagoSolicitud(int id)
        {
            var idAlumno = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return Challenge();
            }

            var solicitud = await _context.SolicitudesCurso
                .Include(s => s.ProfesorPropuesto)
                .Include(s => s.CanchaPropuesta)
                .FirstOrDefaultAsync(s =>
                    s.IdSolicitudCurso == id &&
                    s.IdAlumno == idAlumno);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Propuesta enviada")
            {
                TempData["Error"] =
                    "Esta propuesta ya fue respondida o no está disponible.";

                return RedirectToAction(
                    nameof(ResponderPropuesta),
                    new { id });
            }

            if (!solicitud.PrecioSolicitado.HasValue ||
                solicitud.PrecioSolicitado.Value <= 0)
            {
                TempData["Error"] =
                    "No se encontró un monto válido para esta solicitud.";

                return RedirectToAction(
                    nameof(ConfirmarPagoSolicitud),
                    new { id });
            }

            bool yaExistePago = await _context.Pagos
                .AnyAsync(p =>
                    p.IdAlumno == idAlumno &&
                    p.TipoPago == "Solicitud de clase" &&
                    p.Observaciones != null &&
                    p.Observaciones.Contains(
                        $"SOL-{solicitud.IdSolicitudCurso:D4}") &&
                    p.Estado != "Anulado");

            if (yaExistePago)
            {
                TempData["Error"] =
                    "Ya existe un pago generado para esta solicitud.";

                return RedirectToAction(
                    nameof(ConfirmarPagoSolicitud),
                    new { id });
            }

            decimal distanciaKm = 0;
            decimal costoDesplazamiento = 0;
            int? idUbicacionAlumno = null;

            if (solicitud.EsADomicilio)
            {
                var ubicacion = await _context.UbicacionesAlumno
                    .Include(u => u.Zona)
                    .FirstOrDefaultAsync(u =>
                        u.IdAlumno == idAlumno &&
                        u.EsPrincipal);

                if (ubicacion == null)
                {
                    TempData["Error"] =
                        "Debe registrar una ubicación principal para la clase a domicilio.";

                    return RedirectToAction(
                        "MiUbicacion",
                        "Geolocalizacion");
                }

                if (ubicacion.Zona == null ||
                    !ubicacion.Zona.Activa ||
                    !ubicacion.Zona.LatitudCentro.HasValue ||
                    !ubicacion.Zona.LongitudCentro.HasValue)
                {
                    TempData["Error"] =
                        "La ubicación registrada no tiene una zona de cobertura válida.";

                    return RedirectToAction(
                        nameof(ConfirmarPagoSolicitud),
                        new { id });
                }

                double distanciaCalculada = CalcularDistanciaKm(
                    (double)ubicacion.Latitud,
                    (double)ubicacion.Longitud,
                    (double)ubicacion.Zona.LatitudCentro.Value,
                    (double)ubicacion.Zona.LongitudCentro.Value);

                if (ubicacion.Zona.RadioMaximoKm.HasValue &&
                    distanciaCalculada >
                    (double)ubicacion.Zona.RadioMaximoKm.Value)
                {
                    TempData["Error"] =
                        "La ubicación está fuera del radio permitido para clases a domicilio.";

                    return RedirectToAction(
                        nameof(ConfirmarPagoSolicitud),
                        new { id });
                }

                distanciaKm = Math.Round(
                    Convert.ToDecimal(distanciaCalculada),
                    2);

                decimal tarifaPorKm =
                    ubicacion.Zona.TarifaPorKm ?? 0;

                decimal costoPorDistancia =
                    distanciaKm * tarifaPorKm;

                costoDesplazamiento = Math.Round(
                    ubicacion.Zona.CostoAdicional +
                    costoPorDistancia,
                    2);

                idUbicacionAlumno =
                    ubicacion.IdUbicacion;
            }

            decimal montoBase =
                solicitud.PrecioSolicitado.Value;

            decimal montoTotal =
                montoBase + costoDesplazamiento;

            var pago = new Pago
            {
                IdAlumno = idAlumno,

                MontoBase = montoBase,

                CostoDesplazamiento =
                    costoDesplazamiento,

                Monto =
                    montoTotal,

                EsADomicilio =
                    solicitud.EsADomicilio,

                IdUbicacionAlumno =
                    idUbicacionAlumno,

                DistanciaKm =
                    distanciaKm,

                TipoPago =
                    "Solicitud de clase",

                MetodoPago =
                    "Pendiente",

                Estado =
                    "Pendiente",

                FechaPago =
                    DateTime.Now,

                FechaVencimiento =
                    DateTime.Now.AddDays(3),

                EsManual =
                    false,

                Observaciones =
                    solicitud.EsADomicilio
                        ? $"Pago pendiente para la solicitud " +
                          $"SOL-{solicitud.IdSolicitudCurso:D4} - " +
                          $"{solicitud.NombreCurso}. " +
                          $"Modalidad a domicilio. " +
                          $"Desplazamiento: ₡{costoDesplazamiento:N0}."
                        : $"Pago pendiente para la solicitud " +
                          $"SOL-{solicitud.IdSolicitudCurso:D4} - " +
                          $"{solicitud.NombreCurso}. " +
                          "Modalidad en la academia."
            };

            _context.Pagos.Add(pago);

            solicitud.Estado = "Aceptada";
            solicitud.FechaRespuestaAlumno = DateTime.Now;

            await _context.SaveChangesAsync();

            try
            {
                await CrearNotificacionAdministradoresAsync(
                    solicitud,
                    aceptada: true);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"ERROR NOTIFICACIÓN INTERNA ACEPTACIÓN: {ex}");
            }

            try
            {
                await EnviarCorreoAceptacionAsync(
                    solicitud);
            }
            catch (Exception ex)
            {
                TempData["AdvertenciaCorreo"] =
                    "La propuesta fue aceptada, pero no se pudo enviar el correo a la academia.";

                Console.WriteLine(
                    $"ERROR CORREO ACEPTACIÓN: {ex}");
            }

            TempData["Success"] =
                $"Se generó el pago pendiente por ₡{montoTotal:N0}. " +
                "Adjunta el comprobante para completar el proceso.";

            return RedirectToAction(
                "HistorialPagos",
                "Usuario");
        }


        [HttpPost]
        [Authorize(Roles = "Usuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarPropuesta(int id,string motivo)   
            {

            var idAlumno = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var solicitud = await _context.SolicitudesCurso
        .Include(s => s.ProfesorPropuesto)
        .Include(s => s.CanchaPropuesta)
        .FirstOrDefaultAsync(s =>
            s.IdSolicitudCurso == id &&
            s.IdAlumno == idAlumno);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.Estado != "Propuesta enviada")
            {
                TempData["Error"] =
                    "Esta propuesta ya fue respondida o no está disponible.";

                return RedirectToAction(
                    nameof(ResponderPropuesta),
                    new { id });
            }

            motivo = motivo?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Error"] =
                    "Debes indicar el motivo del rechazo o una nueva disponibilidad.";

                return RedirectToAction(
                    nameof(ResponderPropuesta),
                    new { id });
            }

            if (motivo.Length > 1000)
            {
                TempData["Error"] =
                    "El motivo del rechazo no puede superar los 1000 caracteres.";

                return RedirectToAction(
                    nameof(ResponderPropuesta),
                    new { id });
            }

            solicitud.Estado = "En revisión";
            solicitud.FechaRespuestaAlumno = DateTime.Now;
            solicitud.MotivoRechazoAlumno = motivo;
            await _context.SaveChangesAsync();

            try
            {
                await CrearNotificacionAdministradoresAsync(
                    solicitud,
                    aceptada: false);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"ERROR NOTIFICACIÓN INTERNA RECHAZO: {ex}");
            }


            try
            {
                await EnviarCorreoRechazoAsync(solicitud);
            }
            catch (Exception ex)
            {
                TempData["AdvertenciaCorreo"] =
                    "El rechazo fue registrado, pero no se pudo enviar el correo a la academia.";

                Console.WriteLine($"ERROR CORREO RECHAZO: {ex}");
            }

            var whatsappUrl =
                ConstruirWhatsappRespuestaAlumno(
                    solicitud,
                    aceptada: false);

            if (!string.IsNullOrWhiteSpace(whatsappUrl))
            {
                TempData["WhatsappRespuestaUrl"] =
                    whatsappUrl;
            }
            else
            {
                TempData["AdvertenciaWhatsapp"] =
                    "El rechazo fue registrado, pero no se encontró el número de WhatsApp de la academia.";
            }

            TempData["Success"] =
                "La propuesta fue rechazada. La solicitud regresó a revisión.";

            return RedirectToAction(
                nameof(ResponderPropuesta),
                new { id });
        }
        private static double CalcularDistanciaKm(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double radioTierra = 6371;

            double dLat =
                (lat2 - lat1) * Math.PI / 180;

            double dLon =
                (lon2 - lon1) * Math.PI / 180;

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            double c =
                2 * Math.Atan2(
                    Math.Sqrt(a),
                    Math.Sqrt(1 - a));

            return radioTierra * c;
        }


    }
    }
