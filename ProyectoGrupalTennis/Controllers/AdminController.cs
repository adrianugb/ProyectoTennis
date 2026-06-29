
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models.ViewModels;
using AcademiaTennisDAL.Context;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;

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
    }
}
