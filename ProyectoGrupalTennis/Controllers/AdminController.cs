using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Helpers;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Models.ViewModels;
using ProyectoGrupalTennis.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context,
            EmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();

            var modelo = new List<UsuarioRolViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);

                modelo.Add(new UsuarioRolViewModel
                {
                    Usuario = usuario,
                    Rol = roles.FirstOrDefault() ?? "Sin Rol"
                });
            }

            return View(modelo);
        }

        // HACER PROFESOR
        public async Task<IActionResult> HacerProfesor(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Verifica si ya es profesor
            if (!await _userManager.IsInRoleAsync(usuario, "Profesor"))
            {
                // Quitar rol Usuario
                await _userManager.RemoveFromRoleAsync(usuario, "Usuario");

                // Agregar rol Profesor
                await _userManager.AddToRoleAsync(usuario, "Profesor");
            }

            return RedirectToAction("Index");
        }

        // HACER USUARIO NORMAL
        public async Task<IActionResult> HacerUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Quitar rol Profesor
            await _userManager.RemoveFromRoleAsync(usuario, "Profesor");

            // Agregar rol Usuario
            await _userManager.AddToRoleAsync(usuario, "Usuario");

            return RedirectToAction("Index");
        }

        // BLOQUEAR USUARIO
        public async Task<IActionResult> BloquearUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Bloqueado = true;

            await _userManager.UpdateAsync(usuario);

            return RedirectToAction("Index");
        }

        // DESBLOQUEAR USUARIO
        public async Task<IActionResult> DesbloquearUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Bloqueado = false;

            await _userManager.UpdateAsync(usuario);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CambiarRol(string id, string nuevoRol)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // proteger admin principal
            if (usuario.Email == "admin@tennis.com")
            {
                return RedirectToAction("Index");
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);

            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);

            await _userManager.AddToRoleAsync(usuario, nuevoRol);

            return RedirectToAction("Index");
        }

        // GET: /Admin/AdminPagos
        public async Task<IActionResult> AdminPagos(
            string? buscar,
            string? estado,
            string? factura,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var query = _context.Pagos
              .Include(p => p.Alumno)
              .Include(p => p.Factura)
              .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(p =>
                    (p.Alumno != null &&
                        ((p.Alumno.Nombre + " " + p.Alumno.Apellido).Contains(buscar))) ||
                    p.TipoPago.Contains(buscar));
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

            if (fechaDesde.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date <= fechaHasta.Value.Date);
            }

            var pagos = await query
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            var model = new AdminHistorialPagosViewModel
            {
                FiltroBuscar = buscar,
                FiltroEstado = estado,
                FiltroFactura = factura,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,


                Pagos = pagos.Select(p => new AdminPagoItemViewModel
                {
                    IdPago = p.IdPago,
                    Alumno = p.Alumno != null
                        ? $"{p.Alumno.Nombre} {p.Alumno.Apellido}"
                        : "Sin alumno",
                    Concepto = p.TipoPago,
                    MetodoPago = p.MetodoPago,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    Estado = p.Estado,
                    ComprobantePago = p.ComprobantePago,

                    FacturaEstado = p.Factura != null
                        ? "Disponible"
                        : p.Estado == "Pagado"
                            ? "Pendiente"
                            : "No disponible",
                    FechaFactura = p.Factura != null
                        ? p.Factura.FechaFactura
                        : null
                }).ToList()
            };

            return View("~/Views/Perfiles/AdminPagos.cshtml", model);
        }

      

        // GET: /Admin/ ADMIN-05-007 – Exportar reporte de pago

        public async Task<IActionResult> ExportarReportePagos(
    string? buscar,
    string? estado,
    string? factura,
    DateTime? fechaDesde,
    DateTime? fechaHasta)
        {
            var query = _context.Pagos
                .Include(p => p.Alumno)
                .Include(p => p.Factura)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(p =>
                    (p.Alumno != null &&
                    ((p.Alumno.Nombre + " " + p.Alumno.Apellido).Contains(buscar))) ||
                    p.TipoPago.Contains(buscar));
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

            if (fechaDesde.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date <= fechaHasta.Value.Date);
            }

            var pagos = await query
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            if (!pagos.Any())
            {
                TempData["Error"] = "No existen pagos para exportar.";

                return RedirectToAction(nameof(AdminPagos));
            }
            // PDF
            if (!pagos.Any())
            {
                TempData["Error"] = "No existen pagos para exportar.";
                return RedirectToAction(nameof(AdminPagos));
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte de Pagos");

            worksheet.Cell(1, 1).Value = "REPORTE GENERAL DE PAGOS";
            worksheet.Range(1, 1, 1, 8).Merge().Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 8).Style.Font.FontSize = 16;

            worksheet.Cell(2, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Range(2, 1, 2, 8).Merge();

            worksheet.Cell(4, 1).Value = "ID";
            worksheet.Cell(4, 2).Value = "Alumno";
            worksheet.Cell(4, 3).Value = "Concepto";
            worksheet.Cell(4, 4).Value = "Método";
            worksheet.Cell(4, 5).Value = "Monto";
            worksheet.Cell(4, 6).Value = "Fecha de pago";
            worksheet.Cell(4, 7).Value = "Estado";
            worksheet.Cell(4, 8).Value = "Factura";

            var row = 5;

            foreach (var pago in pagos)
            {
                worksheet.Cell(row, 1).Value = $"PAG-{pago.IdPago}";
                worksheet.Cell(row, 2).Value = pago.Alumno != null
                    ? $"{pago.Alumno.Nombre} {pago.Alumno.Apellido}"
                    : "Sin alumno";
                worksheet.Cell(row, 3).Value = pago.TipoPago;
                worksheet.Cell(row, 4).Value = pago.MetodoPago;
                worksheet.Cell(row, 5).Value = pago.Monto;
                worksheet.Cell(row, 6).Value = pago.FechaPago.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 7).Value = pago.Estado;
                worksheet.Cell(row, 8).Value = pago.Factura != null
                    ? "Disponible"
                    : pago.Estado == "Pagado"
                        ? "Pendiente"
                        : "No disponible";
                row++;
            }

            var headerRange = worksheet.Range(4, 1, 4, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#A3C644");
            headerRange.Style.Font.FontColor = XLColor.White;

            var tableRange = worksheet.Range(4, 1, row - 1, 8);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Column(5).Style.NumberFormat.Format = "\"₡\"#,##0";

            worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_Pagos_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        // GET: /Admin/NotificarAlumnos - ADM-09-002
        public async Task<IActionResult> NotificarAlumnos()
        {
            var model = new NotificarGrupoViewModel
            {
                Cursos = await _context.Cursos
                    .Where(c => c.Activo)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.IdCurso.ToString(),
                        Text = c.Nombre
                    })
                    .ToListAsync()
            };

            return View("~/Views/Perfiles/AdminNotificarGrupo.cshtml", model);
        }

        // POST: /Admin/NotificarAlumnos - ADM-09-002
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotificarAlumnos(NotificarGrupoViewModel model)
        {
            model.Cursos = await _context.Cursos
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.IdCurso.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(model.Titulo) || string.IsNullOrWhiteSpace(model.Mensaje))
            {
                TempData["Error"] = "El título y el mensaje son obligatorios.";
                return View("~/Views/Perfiles/AdminNotificarGrupo.cshtml", model);
            }

            // Determina el grupo: un curso especifico, o todos los alumnos con matricula activa
            List<string> idsAlumnos;

            if (model.IdCurso.HasValue)
            {
                idsAlumnos = await _context.Matriculas
                    .Where(m => m.IdCurso == model.IdCurso.Value && m.Estado == "Activa")
                    .Select(m => m.IdAlumno)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                idsAlumnos = await _context.Matriculas
                    .Where(m => m.Estado == "Activa")
                    .Select(m => m.IdAlumno)
                    .Distinct()
                    .ToListAsync();
            }

            int enviadas = 0;

            foreach (var idAlumno in idsAlumnos)
            {
                var creada = await NotificacionHelper.EnviarNotificacionAsync(
                    _context,
                    _emailService,
                    idAlumno,
                    categoria: "General",
                    tipo: "Aviso administrativo",
                    titulo: model.Titulo,
                    mensaje: model.Mensaje);

                if (creada) enviadas++;
            }

            await _context.SaveChangesAsync();

            model.AlumnosNotificados = enviadas;
            TempData["MensajeExito"] = $"Se envió la notificación a {enviadas} alumno(s).";

            return View("~/Views/Perfiles/AdminNotificarGrupo.cshtml", model);
        }
        
        [HttpGet]
        public async Task<IActionResult> RegistrarPagoManual()
        {
            var model = new RegistrarPagoManualViewModel
            {
                Alumnos = await _context.Users
                    .OrderBy(u => u.Nombre)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.Nombre + " " + u.Apellido + " - " + u.Email
                    })
                    .ToListAsync()
            };

            return View("~/Views/Pagos/RegistrarPagoManual.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPagoManual(RegistrarPagoManualViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Alumnos = await _context.Users
                    .OrderBy(u => u.Nombre)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.Nombre + " " + u.Apellido + " - " + u.Email
                    })
                    .ToListAsync();

                return View("~/Views/Pagos/RegistrarPagoManual.cshtml", model);
            }

            var pago = new Pago
            {
                IdAlumno = model.IdAlumno,
                TipoPago = model.TipoPago,
                Monto = model.Monto,
                MetodoPago = model.MetodoPago,
                Estado = "Pagado",
                FechaPago = DateTime.Now,
                EsManual = true,
                Observaciones = model.Observaciones
            };

            _context.Pagos.Add(pago);

            await NotificacionHelper.EnviarNotificacionAsync(
                _context,
                _emailService,
                pago.IdAlumno,
                categoria: "Pago",
                tipo: "Pago registrado",
                titulo: "Pago registrado por la administración",
                mensaje:
                    $"La administración registró un pago de ₡{pago.Monto:N0} " +
                    $"correspondiente a {pago.TipoPago}, mediante {pago.MetodoPago}."
            );
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Pago manual registrado correctamente.";
            return RedirectToAction(nameof(AdminPagos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPago(int idPago)
        {
            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.IdPago == idPago);

            if (pago == null)
            {
                TempData["MensajeError"] = "No se encontró el pago.";
                return RedirectToAction(nameof(AdminPagos));
            }

            if (pago.Estado != "En revisión")
            {
                TempData["MensajeError"] = "Solo se pueden confirmar pagos en revisión.";
                return RedirectToAction(nameof(AdminPagos));
            }

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            try
            {
                if (pago.TipoPago == "Matricula")
                {
                    if (!pago.IdCurso.HasValue)
                    {
                        TempData["MensajeError"] = "El pago no tiene un curso relacionado.";
                        return RedirectToAction(nameof(AdminPagos));
                    }

                    var curso = await _context.Cursos
                        .FirstOrDefaultAsync(c => c.IdCurso == pago.IdCurso.Value);

                    if (curso == null)
                    {
                        TempData["MensajeError"] = "El curso relacionado al pago no existe.";
                        return RedirectToAction(nameof(AdminPagos));
                    }

                    if (curso.CuposDisponibles <= 0)
                    {
                        TempData["MensajeError"] = "Ya no hay cupos disponibles para este curso.";
                        return RedirectToAction(nameof(AdminPagos));
                    }

                    var yaMatriculado = await _context.Matriculas.AnyAsync(m =>
                        m.IdAlumno == pago.IdAlumno &&
                        m.IdCurso == curso.IdCurso &&
                        m.Estado == "Activa");

                    if (yaMatriculado)
                    {
                        TempData["MensajeError"] = "El alumno ya está matriculado en este curso.";
                        return RedirectToAction(nameof(AdminPagos));
                    }

                    var matricula = new Matricula
                    {
                        IdAlumno = pago.IdAlumno,
                        IdCurso = curso.IdCurso,
                        FechaMatricula = DateTime.Now,
                        Estado = "Activa"
                    };

                    _context.Matriculas.Add(matricula);
                    curso.CuposDisponibles -= 1;

                    await _context.SaveChangesAsync();

                    pago.IdMatricula = matricula.IdMatricula;

                    await NotificacionHelper.EnviarNotificacionAsync(
                        _context,
                        _emailService,
                        pago.IdAlumno,
                        categoria: "Clase",
                        tipo: "Matrícula",
                        titulo: "Matrícula confirmada",
                        mensaje: $"Tu matrícula al curso {curso.Nombre} fue confirmada correctamente."
                    );
                }
                else if (pago.TipoPago == "Reserva")
                {
                    if (!pago.IdReserva.HasValue)
                    {
                        TempData["MensajeError"] = "El pago no tiene una reserva relacionada.";
                        return RedirectToAction(nameof(AdminPagos));
                    }

                    var reserva = await _context.Reservas
                        .Include(r => r.Cancha)
                        .FirstOrDefaultAsync(r => r.IdReserva == pago.IdReserva.Value);

                    if (reserva == null)
                    {
                        TempData["MensajeError"] = "La reserva relacionada al pago no existe.";
                        return RedirectToAction(nameof(AdminPagos));
                    }

                    if (reserva.IdAlumno != null)
                    {
                        TempData["MensajeError"] = "La reserva ya fue tomada por otro alumno.";
                        return RedirectToAction(nameof(AdminPagos));
                    }

                    reserva.IdAlumno = pago.IdAlumno;
                    reserva.Estado = "Asignada";

                    var nombreCancha = reserva.Cancha?.Nombre ?? "la cancha seleccionada";

                    await NotificacionHelper.EnviarNotificacionAsync(
                        _context,
                        _emailService,
                        pago.IdAlumno,
                        categoria: "Clase",
                        tipo: "Reserva",
                        titulo: "Reserva confirmada",
                        mensaje:
                            $"Tu reserva en {nombreCancha} fue confirmada para el " +
                            $"{reserva.FechaReserva:dd/MM/yyyy} de " +
                            $"{reserva.HoraInicio:hh\\:mm} a {reserva.HoraFin:hh\\:mm}."
                    );
                }
                else
                {
                    TempData["MensajeError"] = "El tipo de pago no es válido.";
                    return RedirectToAction(nameof(AdminPagos));
                }

                pago.Estado = "Pagado";
                pago.FechaPago = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["MensajeExito"] = "Pago confirmado correctamente.";
            }
            catch
            {
                await transaccion.RollbackAsync();

                TempData["MensajeError"] =
                    "Ocurrió un error al confirmar el pago. No se realizaron cambios.";
            }

            return RedirectToAction(nameof(AdminPagos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarPago(int idPago)
        {
            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.IdPago == idPago);

            if (pago == null)
            {
                TempData["MensajeError"] = "No se encontró el pago.";
                return RedirectToAction(nameof(AdminPagos));
            }

            if (pago.Estado != "En revisión")
            {
                TempData["MensajeError"] = "Solo se pueden rechazar pagos en revisión.";
                return RedirectToAction(nameof(AdminPagos));
            }

            pago.Estado = "Rechazado";

            await NotificacionHelper.EnviarNotificacionAsync(
                _context,
                _emailService,
                pago.IdAlumno,
                categoria: "Pago",
                tipo: "Pago rechazado",
                titulo: "Comprobante de pago rechazado",
                mensaje:
                    $"El comprobante correspondiente al pago PAG-{pago.IdPago} fue rechazado. " +
                    "Revisa que el monto y los datos sean correctos y vuelve a adjuntar el comprobante."
            );

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "El pago fue rechazado y el alumno recibió una notificación.";

            return RedirectToAction(nameof(AdminPagos));
        }

        // POST: /Admin/GenerarFactura
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarFactura(int idPago)
        {
            var pago = await _context.Pagos
                .Include(p => p.Alumno)
                .Include(p => p.Factura)
                .FirstOrDefaultAsync(p => p.IdPago == idPago);

            if (pago == null)
            {
                TempData["MensajeError"] = "No se encontró el pago.";
                return RedirectToAction(nameof(AdminPagos));
            }

            if (pago.Estado != "Pagado")
            {
                TempData["MensajeError"] = "Solo se pueden generar facturas para pagos confirmados.";
                return RedirectToAction(nameof(AdminPagos));
            }

            if (pago.Factura != null)
            {
                TempData["MensajeError"] = "Este pago ya tiene una factura generada.";
                return RedirectToAction(nameof(AdminPagos));
            }

            var numeroFactura = $"FAC-{DateTime.Now:yyyyMMdd}-{idPago:D4}";

            var factura = new Factura
            {
                IdPago = idPago,
                NumeroFactura = numeroFactura,
                FechaFactura = DateTime.Now
            };

            _context.Facturas.Add(factura);

            await NotificacionHelper.EnviarNotificacionAsync(
                _context,
                _emailService,
                pago.IdAlumno,
                categoria: "Pago",
                tipo: "Factura generada",
                titulo: "Factura disponible",
                mensaje: $"Tu factura {numeroFactura} por ₡{pago.Monto:N0} " +
                         $"correspondiente a {pago.TipoPago} está disponible."
            );

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Factura {numeroFactura} generada correctamente.";
            return RedirectToAction(nameof(AdminPagos));
        }

        // POST: /Admin/AnularPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularPago(int idPago)
        {
            var pago = await _context.Pagos
                .Include(p => p.Factura)
                .Include(p => p.Matricula)
                .FirstOrDefaultAsync(p => p.IdPago == idPago);

            if (pago == null)
            {
                TempData["MensajeError"] = "No se encontró el pago.";
                return RedirectToAction(nameof(AdminPagos));
            }

            if (pago.Estado == "Rechazado")
            {
                TempData["MensajeError"] = "Este pago ya está anulado/rechazado.";
                return RedirectToAction(nameof(AdminPagos));
            }

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            try
            {
                // Si tiene matrícula activa, cancelarla y devolver cupo
                if (pago.IdMatricula.HasValue && pago.Matricula != null)
                {
                    pago.Matricula.Estado = "Cancelada";

                    var curso = await _context.Cursos
                        .FirstOrDefaultAsync(c => c.IdCurso == pago.Matricula.IdCurso);

                    if (curso != null)
                        curso.CuposDisponibles += 1;
                }

                // Si tiene factura, eliminarla
                if (pago.Factura != null)
                {
                    _context.Facturas.Remove(pago.Factura);
                }

                pago.Estado = "Rechazado";

                await NotificacionHelper.EnviarNotificacionAsync(
                    _context,
                    _emailService,
                    pago.IdAlumno,
                    categoria: "Pago",
                    tipo: "Pago anulado",
                    titulo: "Pago anulado",
                    mensaje: $"El pago PAG-{pago.IdPago} por ₡{pago.Monto:N0} " +
                             $"correspondiente a {pago.TipoPago} fue anulado por la administración. " +
                             "Contactá a la academia si tenés dudas."
                );

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["MensajeExito"] = "Pago anulado correctamente.";
            }
            catch
            {
                await transaccion.RollbackAsync();
                TempData["MensajeError"] = "Ocurrió un error al anular el pago.";
            }

            return RedirectToAction(nameof(AdminPagos));
        }

        // GET: /Admin/DescargarFactura/5
        public async Task<IActionResult> DescargarFactura(int idPago)
        {
            var pago = await _context.Pagos
                .Include(p => p.Alumno)
                .Include(p => p.Factura)
                .FirstOrDefaultAsync(p => p.IdPago == idPago);

            if (pago?.Factura == null)
            {
                TempData["MensajeError"] = "Factura no encontrada.";
                return RedirectToAction(nameof(AdminPagos));
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("ACADEMIA DE TENIS M.M.P")
                            .FontSize(20).Bold().FontColor("#a5c422");
                        col.Item().Text("Factura Electrónica")
                            .FontSize(14).FontColor("#555555");
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor("#a5c422");
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"No. Factura: {pago.Factura.NumeroFactura}").Bold();
                                c.Item().Text($"Fecha: {pago.Factura.FechaFactura:dd/MM/yyyy}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"ID Pago: PAG-{pago.IdPago}").Bold();
                                c.Item().Text($"Estado: {pago.Estado}");
                            });
                        });

                        col.Item().PaddingTop(20).Text("DATOS DEL CLIENTE").Bold().FontSize(12);
                        col.Item().LineHorizontal(0.5f).LineColor("#cccccc");
                        col.Item().PaddingTop(5).Text(
                            $"{pago.Alumno?.Nombre} {pago.Alumno?.Apellido}");
                        col.Item().Text($"Email: {pago.Alumno?.Email}");

                        col.Item().PaddingTop(20).Text("DETALLE DEL PAGO").Bold().FontSize(12);
                        col.Item().LineHorizontal(0.5f).LineColor("#cccccc");

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#a5c422").Padding(5)
                                    .Text("Concepto").FontColor("#ffffff").Bold();
                                header.Cell().Background("#a5c422").Padding(5)
                                    .Text("Método").FontColor("#ffffff").Bold();
                                header.Cell().Background("#a5c422").Padding(5)
                                    .Text("Monto").FontColor("#ffffff").Bold();
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor("#eeeeee").Padding(5)
                                .Text(pago.TipoPago);
                            table.Cell().BorderBottom(0.5f).BorderColor("#eeeeee").Padding(5)
                                .Text(pago.MetodoPago);
                            table.Cell().BorderBottom(0.5f).BorderColor("#eeeeee").Padding(5)
                                .Text($"₡{pago.Monto:N0}");
                        });

                        col.Item().PaddingTop(15).AlignRight()
                            .Text($"TOTAL: ₡{pago.Monto:N0}").Bold().FontSize(14);

                        if (!string.IsNullOrEmpty(pago.Observaciones))
                        {
                            col.Item().PaddingTop(20).Text("Observaciones:").Bold();
                            col.Item().Text(pago.Observaciones).FontColor("#555555");
                        }
                    });

                    page.Footer().AlignCenter()
                        .Text($"Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm} — Academia de Tenis M.M.P")
                        .FontSize(9).FontColor("#aaaaaa");
                });
            });

            var bytes = pdf.GeneratePdf();
            return File(bytes, "application/pdf",
                $"Factura_{pago.Factura.NumeroFactura}.pdf");
        }

        // GET: /Admin/AdminFacturas 
        public async Task<IActionResult> AdminFacturas(string? buscar)
        {
            var query = _context.Facturas
                .Include(f => f.Pago)
                .ThenInclude(p => p.Alumno)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(f =>
                    f.NumeroFactura.Contains(buscar) ||
                    (f.Pago.Alumno != null &&
                     (f.Pago.Alumno.Nombre + " " + f.Pago.Alumno.Apellido).Contains(buscar)));
            }

            var facturas = await query
                .OrderByDescending(f => f.FechaFactura)
                .ToListAsync();

            var vm = facturas.Select(f => new AdminFacturaItemViewModel
            {
                IdFactura = f.IdFactura,
                IdPago = f.IdPago,
                NumeroFactura = f.NumeroFactura,
                Alumno = f.Pago.Alumno != null
                    ? $"{f.Pago.Alumno.Nombre} {f.Pago.Alumno.Apellido}"
                    : "Sin alumno",
                Concepto = f.Pago.TipoPago,
                Monto = f.Pago.Monto,
                FechaFactura = f.FechaFactura
            }).ToList();

            ViewBag.FiltroBuscar = buscar;
            return View("~/Views/Perfiles/AdminFacturas.cshtml", vm);
        }
    }

}