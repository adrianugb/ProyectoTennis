using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CanchasController : Controller
    {
        private readonly AppDbContext _context;

        public CanchasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Canchas/Index
        // ADM-04-001: Visualizar canchas con filtros
        public async Task<IActionResult> Index(string? buscar, string? estado)
        {
            var query = _context.Canchas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(c => c.Nombre.Contains(buscar));

            if (estado == "Disponible")
                query = query.Where(c => c.Disponible && !c.EnMantenimiento);
            else if (estado == "Mantenimiento")
                query = query.Where(c => c.EnMantenimiento);
            else if (estado == "NoDisponible")
                query = query.Where(c => !c.Disponible && !c.EnMantenimiento);

            var canchas = await query.OrderBy(c => c.Nombre).ToListAsync();

            var viewModel = new AdminCanchasViewModel
            {
                FiltroBuscar = buscar,
                FiltroEstado = estado,
                MensajeExito = TempData["MensajeExito"]?.ToString(),
                MensajeError = TempData["MensajeError"]?.ToString(),
                Canchas = canchas.Select(c => new CanchaItemViewModel
                {
                    IdCancha = c.IdCancha,
                    Nombre = c.Nombre,
                    Disponible = c.Disponible,
                    EnMantenimiento = c.EnMantenimiento
                }).ToList()
            };

            return View("~/Views/Perfiles/AdminCanchas.cshtml", viewModel);
        }

        // POST: /Canchas/Crear
        // ADM-04-001: Registrar nueva cancha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AdminCanchasViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Por favor completá todos los campos obligatorios.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar duplicado
            var existe = await _context.Canchas
                .AnyAsync(c => c.Nombre.ToLower() == model.NuevaCancha.Nombre.ToLower());

            if (existe)
            {
                TempData["MensajeError"] = $"Ya existe una cancha con el nombre '{model.NuevaCancha.Nombre}'.";
                return RedirectToAction(nameof(Index));
            }

            var cancha = new Cancha
            {
                Nombre = model.NuevaCancha.Nombre.Trim(),
                Disponible = model.NuevaCancha.Disponible,
                EnMantenimiento = false
            };

            _context.Canchas.Add(cancha);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cancha '{cancha.Nombre}' registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Canchas/ObtenerDatos/5
        // AJAX para cargar datos en modal de edición
        [HttpGet]
        public async Task<IActionResult> ObtenerDatos(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);

            if (cancha == null)
                return Json(new { exito = false, mensaje = "La cancha no existe en el sistema." });

            return Json(new
            {
                exito = true,
                idCancha = cancha.IdCancha,
                nombre = cancha.Nombre,
                disponible = cancha.Disponible
            });
        }

        // POST: /Canchas/Editar
        // ADM-04-001: Editar cancha existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarCanchaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Datos inválidos. Revisá los campos.";
                return RedirectToAction(nameof(Index));
            }

            var cancha = await _context.Canchas.FindAsync(model.IdCancha);

            if (cancha == null)
            {
                TempData["MensajeError"] = "La cancha no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            cancha.Nombre = model.Nombre.Trim();
            cancha.Disponible = model.Disponible;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cancha '{cancha.Nombre}' actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Canchas/Mantenimiento
        // ADM-04-002: Marcar/desmarcar cancha en mantenimiento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mantenimiento(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);

            if (cancha == null)
            {
                TempData["MensajeError"] = "La cancha no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            if (cancha.EnMantenimiento)
            {
                TempData["MensajeError"] = $"La cancha '{cancha.Nombre}' ya se encuentra en mantenimiento.";
                return RedirectToAction(nameof(Index));
            }

            cancha.EnMantenimiento = true;
            cancha.Disponible = false;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cancha '{cancha.Nombre}' marcada como en mantenimiento.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Canchas/FinalizarMantenimiento
        // Quitar mantenimiento y volver a disponible
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarMantenimiento(int id)
        {
            var cancha = await _context.Canchas.FindAsync(id);

            if (cancha == null)
            {
                TempData["MensajeError"] = "La cancha no existe en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            cancha.EnMantenimiento = false;
            cancha.Disponible = true;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cancha '{cancha.Nombre}' disponible nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}