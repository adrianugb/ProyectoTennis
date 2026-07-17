using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminTarifasController : Controller
    {
        private readonly AppDbContext _context;

        public AdminTarifasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /AdminTarifas
        public async Task<IActionResult> Index(
            string? buscar,
            int? idTipoClase,
            string? estado)
        {
            var query = _context.TarifasClase
                .Include(t => t.TipoClase)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(t =>
                    t.Nombre.Contains(buscar) ||
                    (t.Descripcion != null &&
                     t.Descripcion.Contains(buscar)));
            }

            if (idTipoClase.HasValue)
            {
                query = query.Where(t =>
                    t.IdTipoClase == idTipoClase.Value);
            }

            if (estado == "Activo")
            {
                query = query.Where(t => t.Activa);
            }
            else if (estado == "Inactivo")
            {
                query = query.Where(t => !t.Activa);
            }

            ViewBag.TiposClase = new SelectList(
                await _context.TiposClase
                    .OrderBy(t => t.Nombre)
                    .ToListAsync(),
                "IdTipoClase",
                "Nombre",
                idTipoClase
            );

            var tarifas = await query
                .OrderBy(t => t.TipoClase.Nombre)
                .ThenBy(t => t.CantidadLecciones)
                .ThenBy(t => t.Precio)
                .ToListAsync();

            return View(tarifas);
        }

        // GET: /AdminTarifas/Agregar
        [HttpGet]
        public async Task<IActionResult> Agregar()
        {
            await CargarTiposClaseAsync();

            return View(new TarifaClase
            {
                Activa = true,
                CantidadLecciones = 1
            });
        }

        // POST: /AdminTarifas/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(TarifaClase tarifa)
        {
            if (!await _context.TiposClase
                    .AnyAsync(t =>
                        t.IdTipoClase == tarifa.IdTipoClase &&
                        t.Activo))
            {
                ModelState.AddModelError(
                    nameof(tarifa.IdTipoClase),
                    "Debe seleccionar un tipo de clase activo.");
            }

            if (!ModelState.IsValid)
            {
                await CargarTiposClaseAsync(tarifa.IdTipoClase);
                return View(tarifa);
            }

            tarifa.IdTarifaClase = 0;
            tarifa.TipoClase = null!;
            tarifa.FechaActualizacion = DateTime.Now;

            _context.TarifasClase.Add(tarifa);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La tarifa fue agregada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminTarifas/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var tarifa = await _context.TarifasClase
                .FirstOrDefaultAsync(t =>
                    t.IdTarifaClase == id);

            if (tarifa == null)
            {
                return NotFound();
            }

            await CargarTiposClaseAsync(
                tarifa.IdTipoClase,
                incluirTipoInactivoSeleccionado: true);

            return View(tarifa);
        }

        // POST: /AdminTarifas/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(TarifaClase tarifa)
        {
            var tipoValido = await _context.TiposClase
                .AnyAsync(t =>
                    t.IdTipoClase == tarifa.IdTipoClase &&
                    t.Activo);

            if (!tipoValido)
            {
                ModelState.AddModelError(
                    nameof(tarifa.IdTipoClase),
                    "Debe seleccionar un tipo de clase activo.");
            }

            if (!ModelState.IsValid)
            {
                await CargarTiposClaseAsync(
                    tarifa.IdTipoClase,
                    incluirTipoInactivoSeleccionado: true);

                return View(tarifa);
            }

            var tarifaExistente =
                await _context.TarifasClase
                    .FirstOrDefaultAsync(t =>
                        t.IdTarifaClase ==
                        tarifa.IdTarifaClase);

            if (tarifaExistente == null)
            {
                return NotFound();
            }

            tarifaExistente.Nombre = tarifa.Nombre;
            tarifaExistente.IdTipoClase =
                tarifa.IdTipoClase;
            tarifaExistente.CondicionMatricula =
                tarifa.CondicionMatricula;
            tarifaExistente.CantidadLecciones =
                tarifa.CantidadLecciones;
            tarifaExistente.Precio = tarifa.Precio;
            tarifaExistente.PrecioPorPersona =
                tarifa.PrecioPorPersona;
            tarifaExistente.Descripcion =
                tarifa.Descripcion;
            tarifaExistente.Activa = tarifa.Activa;
            tarifaExistente.FechaActualizacion =
                DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La tarifa fue actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminTarifas/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id,
            bool activa)
        {
            var tarifa = await _context.TarifasClase
                .FirstOrDefaultAsync(t =>
                    t.IdTarifaClase == id);

            if (tarifa == null)
            {
                return NotFound();
            }

            tarifa.Activa = activa;
            tarifa.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = activa
                ? "La tarifa fue activada correctamente."
                : "La tarifa fue desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarTiposClaseAsync(
            int? idSeleccionado = null,
            bool incluirTipoInactivoSeleccionado = false)
        {
            var query = _context.TiposClase.AsQueryable();

            if (incluirTipoInactivoSeleccionado &&
                idSeleccionado.HasValue)
            {
                query = query.Where(t =>
                    t.Activo ||
                    t.IdTipoClase == idSeleccionado.Value);
            }
            else
            {
                query = query.Where(t => t.Activo);
            }

            var tipos = await query
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.TiposClase = new SelectList(
                tipos,
                "IdTipoClase",
                "Nombre",
                idSeleccionado
            );
        }
    }
}