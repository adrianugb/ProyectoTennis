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
                return;
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
                                    {(tarifa.CantidadLecciones == 1 ? "sesión" : "sesiones")}    
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

            Console.WriteLine("===========");
            Console.WriteLine("ENTRÓ A EnviarCorreoRechazoAsync");
            Console.WriteLine($"Destino: {_configuration["AcademiaSettings:CorreoSolicitudes"]}");
            Console.WriteLine("===========");
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

            Console.WriteLine("ANTES DE ENVIAR EL CORREO");

            await _emailService.EnviarCorreoAsync(
                correoDestino,
                $"Propuesta rechazada - SOL-{solicitud.IdSolicitudCurso:D4}",
                cuerpoCorreo);

            Console.WriteLine("CORREO ENVIADO");

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

            solicitud.Estado = "Aceptada";
            solicitud.FechaRespuestaAlumno = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La propuesta fue aceptada correctamente.";

            return RedirectToAction(
                nameof(ResponderPropuesta),
                new { id });
        }
        [HttpPost]
        [Authorize(Roles = "Usuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarPropuesta(
    int id,
    string motivo)
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
                await EnviarCorreoRechazoAsync(solicitud);
            }
            catch (Exception ex)
            {
                TempData["AdvertenciaCorreo"] =
                    "El rechazo fue registrado, pero no se pudo enviar el correo a la academia.";

                Console.WriteLine($"ERROR CORREO RECHAZO: {ex}");
            }

            TempData["Success"] =
                "La propuesta fue rechazada. La solicitud regresó a revisión.";

            return RedirectToAction(
                nameof(ResponderPropuesta),
                new { id });
        }

    }
    }
