
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models.ViewModels;
using AcademiaTennisDAL.Context;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using ClosedXML.Excel;
using System.IO;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
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
    }
}
