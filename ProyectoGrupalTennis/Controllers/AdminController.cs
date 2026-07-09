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

        // GET: /Admin/AdminFacturas
        public IActionResult AdminFacturas()
        {
            return View("~/Views/Perfiles/AdminFacturas.cshtml");
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
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Pago manual registrado correctamente.";
            return RedirectToAction(nameof(AdminPagos));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPago(int idPago)
        {
            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                    .ThenInclude(r => r.Cancha)
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

            if (pago.TipoPago == "Matricula")
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.IdCurso == pago.IdCurso);

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
            }
            else if (pago.TipoPago == "Reserva")
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Cancha)
                    .FirstOrDefaultAsync(r => r.IdReserva == pago.IdReserva);

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
            }

            pago.Estado = "Pagado";
            pago.FechaPago = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Pago confirmado correctamente.";
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

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Pago rechazado correctamente.";

            return RedirectToAction(nameof(AdminPagos));
        }
    }
}