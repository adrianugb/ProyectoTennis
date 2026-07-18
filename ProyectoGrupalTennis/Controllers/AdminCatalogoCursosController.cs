using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminCatalogoCursosController : Controller
    {
        private readonly AppDbContext _context;

        public AdminCatalogoCursosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /AdminCatalogoCursos
        public async Task<IActionResult> Index(
            string? buscarTipo,
            string? estadoTipo)
        {
            var tiposQuery = _context.TiposClase.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscarTipo))
            {
                tiposQuery = tiposQuery.Where(t =>
                    t.Nombre.Contains(buscarTipo) ||
                    (t.Descripcion != null &&
                     t.Descripcion.Contains(buscarTipo)));
            }

            if (estadoTipo == "Activo")
            {
                tiposQuery = tiposQuery.Where(t => t.Activo);
            }
            else if (estadoTipo == "Inactivo")
            {
                tiposQuery = tiposQuery.Where(t => !t.Activo);
            }

            // Información para las distintas secciones de la misma página.
            ViewBag.TiposClase = await tiposQuery
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.Tarifas = await _context.TarifasClase
                .Include(t => t.TipoClase)
                .OrderBy(t => t.TipoClase.Nombre)
                .ThenBy(t => t.CantidadLecciones)
                .ToListAsync();

            ViewBag.CondicionesServicio =
                await _context.CondicionesServicio
                    .OrderBy(c => c.Orden)
                    .ThenBy(c => c.Titulo)
                    .ToListAsync();

            return View();
        }

        // GET: /AdminCatalogoCursos/AgregarTipoClase
        [HttpGet]
        public IActionResult AgregarTipoClase()
        {
            return View(new TipoClase
            {
                Activo = true
            });
        }

        // POST: /AdminCatalogoCursos/AgregarTipoClase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarTipoClase(
            TipoClase tipoClase)
        {
            tipoClase.Nombre = tipoClase.Nombre?.Trim()
                ?? string.Empty;

            var nombreNormalizado =
                tipoClase.Nombre.ToLower();

            var existe = await _context.TiposClase
                .AnyAsync(t =>
                    t.Nombre.ToLower() == nombreNormalizado);

            if (existe)
            {
                ModelState.AddModelError(
                    nameof(tipoClase.Nombre),
                    "Ya existe un tipo de clase con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                return View(tipoClase);
            }

            tipoClase.IdTipoClase = 0;
            tipoClase.FechaActualizacion = DateTime.Now;

            _context.TiposClase.Add(tipoClase);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El tipo de clase fue agregado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminCatalogoCursos/EditarTipoClase/5
        [HttpGet]
        public async Task<IActionResult> EditarTipoClase(int id)
        {
            var tipoClase = await _context.TiposClase
                .FirstOrDefaultAsync(t =>
                    t.IdTipoClase == id);

            if (tipoClase == null)
            {
                return NotFound();
            }

            return View(tipoClase);
        }

        // POST: /AdminCatalogoCursos/EditarTipoClase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTipoClase(
            TipoClase tipoClase)
        {
            tipoClase.Nombre = tipoClase.Nombre?.Trim()
                ?? string.Empty;

            var nombreNormalizado =
                tipoClase.Nombre.ToLower();

            var nombreRepetido = await _context.TiposClase
                .AnyAsync(t =>
                    t.IdTipoClase != tipoClase.IdTipoClase &&
                    t.Nombre.ToLower() == nombreNormalizado);

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(tipoClase.Nombre),
                    "Ya existe otro tipo de clase con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                return View(tipoClase);
            }

            var tipoExistente = await _context.TiposClase
                .FirstOrDefaultAsync(t =>
                    t.IdTipoClase == tipoClase.IdTipoClase);

            if (tipoExistente == null)
            {
                return NotFound();
            }

            tipoExistente.Nombre = tipoClase.Nombre;
            tipoExistente.Descripcion =
                tipoClase.Descripcion;
            tipoExistente.Activo = tipoClase.Activo;
            tipoExistente.FechaActualizacion =
                DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El tipo de clase fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoTipoClase(int id)
        {
            var tipoClase = await _context.TiposClase
                .FirstOrDefaultAsync(t => t.IdTipoClase == id);

            if (tipoClase == null)
            {
                return NotFound();
            }

            // Cambia automáticamente al estado contrario.
            tipoClase.Activo = !tipoClase.Activo;
            tipoClase.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = tipoClase.Activo
                ? "El tipo de clase fue activado correctamente."
                : "El tipo de clase fue desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        // GET: /AdminCatalogoCursos/AgregarCondicionServicio
        [HttpGet]
        public async Task<IActionResult> AgregarCondicionServicio()
        {
            var siguienteOrden = await _context.CondicionesServicio.AnyAsync()
                ? await _context.CondicionesServicio.MaxAsync(c => c.Orden) + 1
                : 1;

            return View(new CondicionServicio
            {
                Activa = true,
                Orden = siguienteOrden
            });
        }

        // POST: /AdminCatalogoCursos/AgregarCondicionServicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarCondicionServicio(
            CondicionServicio condicion)
        {
            condicion.Titulo = condicion.Titulo?.Trim()
                ?? string.Empty;

            condicion.Descripcion = condicion.Descripcion?.Trim()
                ?? string.Empty;

            var existe = await _context.CondicionesServicio
                .AnyAsync(c =>
                    c.Titulo.ToLower() ==
                    condicion.Titulo.ToLower());

            if (existe)
            {
                ModelState.AddModelError(
                    nameof(condicion.Titulo),
                    "Ya existe una condición con ese título.");
            }

            if (!ModelState.IsValid)
            {
                return View(condicion);
            }

            condicion.IdCondicionServicio = 0;
            condicion.FechaActualizacion = DateTime.Now;

            _context.CondicionesServicio.Add(condicion);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La condición del servicio fue agregada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminCatalogoCursos/EditarCondicionServicio/5
        [HttpGet]
        public async Task<IActionResult> EditarCondicionServicio(int id)
        {
            var condicion = await _context.CondicionesServicio
                .FirstOrDefaultAsync(c =>
                    c.IdCondicionServicio == id);

            if (condicion == null)
            {
                return NotFound();
            }

            return View(condicion);
        }

        // POST: /AdminCatalogoCursos/EditarCondicionServicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCondicionServicio(
            CondicionServicio condicion)
        {
            condicion.Titulo = condicion.Titulo?.Trim()
                ?? string.Empty;

            condicion.Descripcion = condicion.Descripcion?.Trim()
                ?? string.Empty;

            var tituloRepetido =
                await _context.CondicionesServicio
                    .AnyAsync(c =>
                        c.IdCondicionServicio !=
                            condicion.IdCondicionServicio &&
                        c.Titulo.ToLower() ==
                            condicion.Titulo.ToLower());

            if (tituloRepetido)
            {
                ModelState.AddModelError(
                    nameof(condicion.Titulo),
                    "Ya existe otra condición con ese título.");
            }

            if (!ModelState.IsValid)
            {
                return View(condicion);
            }

            var condicionExistente =
                await _context.CondicionesServicio
                    .FirstOrDefaultAsync(c =>
                        c.IdCondicionServicio ==
                        condicion.IdCondicionServicio);

            if (condicionExistente == null)
            {
                return NotFound();
            }

            condicionExistente.Titulo = condicion.Titulo;
            condicionExistente.Descripcion =
                condicion.Descripcion;
            condicionExistente.Activa = condicion.Activa;
            condicionExistente.FechaActualizacion =
                DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La condición del servicio fue actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminCatalogoCursos/CambiarEstadoCondicionServicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoCondicionServicio(
            int id)
        {
            var condicion = await _context.CondicionesServicio
                .FirstOrDefaultAsync(c =>
                    c.IdCondicionServicio == id);

            if (condicion == null)
            {
                return NotFound();
            }

            condicion.Activa = !condicion.Activa;
            condicion.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = condicion.Activa
                ? "La condición del servicio fue activada correctamente."
                : "La condición del servicio fue desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        // POST: /AdminCatalogoCursos/ReordenarCondiciones
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReordenarCondiciones(
            [FromForm] List<int> ordenIds)
        {
            if (ordenIds == null || ordenIds.Count == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se recibió el nuevo orden."
                });
            }

            var idsUnicos = ordenIds.Distinct().ToList();

            var condiciones = await _context.CondicionesServicio
                .Where(c => idsUnicos.Contains(c.IdCondicionServicio))
                .ToListAsync();

            if (condiciones.Count != idsUnicos.Count)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Una o más condiciones no fueron encontradas."
                });
            }

            for (int i = 0; i < ordenIds.Count; i++)
            {
                var condicion = condiciones.First(c =>
                    c.IdCondicionServicio == ordenIds[i]);

                condicion.Orden = i + 1;
                condicion.FechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "El orden fue actualizado correctamente."
            });
        }
    }


}