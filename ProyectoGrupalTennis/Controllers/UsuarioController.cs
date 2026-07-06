using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;
using static System.Net.Mime.MediaTypeNames;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public UsuarioController(AppDbContext context, EmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
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

            var notificacion = new Notificacion
            {
                IdUsuario = userId,
                Tipo = "Matrícula",
                Titulo = "Matrícula confirmada",
                Mensaje = $"Tu matrícula al curso {curso.Nombre} fue confirmada correctamente.",
                Leida = false,
                FechaEnvio = DateTime.Now
            };

            _context.Notificaciones.Add(notificacion);

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


        // GET: /Usuario/CatalogoCursos
        public IActionResult CatalogoCursos()
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
                Detalle = "Puede ser individual, parejas o grupos.",
                Imagen = "/images/clases-jubilados.jpg"
            }
        }
            };

            return View("~/Views/Perfiles/CatalogoCursos.cshtml", model);
        }

        // GET: /Usuario/SolicitarCurso
        public IActionResult SolicitarCurso(string nombreCurso)
        {
            var model = new SolicitudCursoViewModel
            {
                NombreCurso = nombreCurso
            };

            return View("~/Views/Perfiles/SolicitarCurso.cshtml", model);
        }


        public async Task<IActionResult> ConfirmacionSolicitud(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var solicitud = await _context.SolicitudesCurso
                .FirstOrDefaultAsync(s => s.IdSolicitudCurso == id && s.IdAlumno == userId);

            if (solicitud == null)
                return NotFound();

            var model = new SolicitudCursoConfirmacionViewModel
            {
                NombreCurso = solicitud.NombreCurso,
                Nivel = solicitud.Nivel,
                Disponibilidad = solicitud.Disponibilidad,
                Comentarios = solicitud.Comentarios,
                FechaSolicitud = solicitud.FechaSolicitud
            };

            return View("~/Views/Perfiles/ConfirmacionSolicitud.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarCurso(SolicitudCursoViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Perfiles/SolicitarCurso.cshtml", model);

            var idAlumno = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var solicitud = new SolicitudCurso
            {
                IdAlumno = idAlumno!,
                IdCurso = null,
                NombreCurso = model.NombreCurso,
                Nivel = model.Nivel,
                Disponibilidad = model.Disponibilidad,
                Comentarios = model.Comentarios,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.Now
            };

            _context.SolicitudesCurso.Add(solicitud);
            await _context.SaveChangesAsync();

            var correoDestino = _configuration["AcademiaSettings:CorreoSolicitudes"];

            if (!string.IsNullOrWhiteSpace(correoDestino))
            {
                await _emailService.EnviarCorreoAsync(
                    correoDestino,
                    "Nueva solicitud de curso",
                    $"Curso: {model.NombreCurso}<br>" +
                    $"Nivel: {model.Nivel}<br>" +
                    $"Disponibilidad: {model.Disponibilidad}<br>" +
                    $"Comentarios: {model.Comentarios}"
                );
            }

            return RedirectToAction(nameof(ConfirmacionSolicitud), new { id = solicitud.IdSolicitudCurso });

        }


        // GET: /Usuario/HistorialPagos USER-05-003 
        public async Task<IActionResult> HistorialPagos(
            string? buscar,
            string? estado,
            string? factura)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Pagos
                .Include(p => p.Factura)
                .Where(p => p.IdAlumno == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(p =>
                    p.TipoPago.Contains(buscar) ||
                    p.MetodoPago.Contains(buscar));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(p => p.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(factura))
            {
                query = factura switch
                {
                    "Disponible" => query.Where(p => p.Factura != null),

                    "Pendiente" => query.Where(p =>
                        p.Factura == null &&
                        p.Estado == "Pagado"),

                    "No disponible" => query.Where(p =>
                        p.Factura == null &&
                        p.Estado != "Pagado"),

                    _ => query
                };
            }

            var pagos = await query
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            var model = new UsuarioHistorialPagosViewModel
            {
                FiltroBuscar = buscar,
                FiltroEstado = estado,
                FiltroFactura = factura,

                Pagos = pagos.Select(p => new UsuarioPagoItemViewModel
                {
                    IdPago = p.IdPago,
                    Concepto = p.TipoPago,
                    MetodoPago = p.MetodoPago,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    FechaFactura = p.Factura != null ? p.Factura.FechaFactura : null,
                    NumeroFactura = p.Factura != null ? p.Factura.NumeroFactura : null,
                    Estado = p.Estado
                }).ToList()
            };

            return View("~/Views/Perfiles/UsuarioHistorialPagos.cshtml", model);
        }


        // GET: /Usuario/DescargarComprobante/ USER-05-004 – Descargar comprobante de pago

        [HttpGet]
        public async Task<IActionResult> DescargarComprobante(int idPago)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var pago = await _context.Pagos
                .Include(p => p.Alumno)
                .Include(p => p.Factura)
                .FirstOrDefaultAsync(p => p.IdPago == idPago && p.IdAlumno == userId);

            if (pago == null)
            {
                TempData["Error"] = "No se encontró el pago seleccionado.";
                return RedirectToAction(nameof(HistorialPagos));
            }

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo-mmp.png");

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(45);
                    page.Size(PageSizes.A4);

                    page.Header().Row(row =>
                    {
                        if (System.IO.File.Exists(logoPath))
                        {
                            row.RelativeItem().Height(70).Image(logoPath).FitHeight();
                        }

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("COMPROBANTE DE PAGO").FontSize(20).Bold();
                            col.Item().Text($"Pago No. PAG-{pago.IdPago}").FontSize(10);
                            col.Item().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy}").FontSize(10);
                        });
                    });

                    page.Content().PaddingTop(30).Column(col =>
                    {
                        col.Spacing(14);

                        col.Item().Text("Datos del alumno").FontSize(14).Bold();
                        col.Item().Text($"Alumno: {pago.Alumno.Nombre} {pago.Alumno.Apellido}");
                        col.Item().Text($"Correo: {pago.Alumno.Email}");

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Detalle del pago").FontSize(14).Bold();
                        col.Item().Text($"Concepto: {pago.TipoPago}");
                        col.Item().Text($"Método de pago: {pago.MetodoPago}");
                        col.Item().Text($"Fecha de pago: {pago.FechaPago:dd/MM/yyyy}");
                        col.Item().Text($"Estado del pago: {pago.Estado}");

                        col.Item().Text($"Monto cancelado: ₡{pago.Monto:N0}")
                            .FontSize(16)
                            .Bold();

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Datos de factura").FontSize(14).Bold();

                        if (pago.Factura != null)
                        {
                            col.Item().Text($"Número de factura: {pago.Factura.NumeroFactura}");
                            col.Item().Text($"Fecha de factura: {pago.Factura.FechaFactura:dd/MM/yyyy}");
                        }
                        else
                        {
                            col.Item().Text("Factura: Pendiente de emisión");
                        }

                        if (!string.IsNullOrWhiteSpace(pago.Observaciones))
                        {
                            col.Item().LineHorizontal(1);
                            col.Item().Text("Observaciones").FontSize(14).Bold();
                            col.Item().Text(pago.Observaciones);
                        }

                        col.Item().PaddingTop(25).Text(
                            "Este documento corresponde a un comprobante de pago generado por el sistema. No sustituye la factura electrónica emitida mediante el sistema del Ministerio de Hacienda."
                        ).FontSize(9).Italic();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Academia de Tennis M.M.P. | Comprobante generado automáticamente")
                        .FontSize(9);
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Comprobante_PAG-{pago.IdPago}.pdf");
        }

        // GET: /Usuario/Notificaciones - USER-09-010 – Marcar notificación como leída
        public async Task<IActionResult> Notificaciones()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var notificaciones = await _context.Notificaciones
                .Where(n => n.IdUsuario == userId)
                .OrderByDescending(n => n.FechaEnvio)
                .ToListAsync();

            var model = new NotificacionesUsuarioViewModel
            {
                Notificaciones = notificaciones.Select(n => new NotificacionUsuarioItemViewModel
                {
                    IdNotificacion = n.IdNotificacion,
                    Tipo = n.Tipo,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    Leida = n.Leida,
                    FechaEnvio = n.FechaEnvio
                }).ToList()
            };

            return View("~/Views/Notificaciones/_NotificacionesUsuario.cshtml", model);
        }

        // POST: /Usuario/MarcarNotificacionLeida
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarNotificacionLeida(int idNotificacion)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n =>
                    n.IdNotificacion == idNotificacion &&
                    n.IdUsuario == userId);

            if (notificacion == null)
            {
                TempData["Error"] = "No se encontró la notificación seleccionada.";
                return RedirectToAction(nameof(Notificaciones));
            }

            notificacion.Leida = true;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La notificación fue marcada como leída.";

            return RedirectToAction(nameof(Notificaciones));
        }
    }
}