using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class GeolocalizacionController : Controller
    {
        private readonly AppDbContext _context;

        public GeolocalizacionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AdminGeolocalizacion
        public async Task<IActionResult> Index()
        {
            var zonas = await _context.ZonasCobertura
                .OrderBy(z => z.Nombre)
                .ToListAsync();

            return View(zonas);
        }

        // GET: AdminGeolocalizacion/Crear
        public IActionResult Crear()
        {
            return View(new ZonaCobertura());
        }

        // POST: AdminGeolocalizacion/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ZonaCobertura zona)
        {
            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            bool nombreRepetido = await _context.ZonasCobertura
                .AnyAsync(z => z.Nombre.ToLower() == zona.Nombre.ToLower());

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(zona.Nombre),
                    "Ya existe una zona de cobertura con ese nombre.");

                return View(zona);
            }

            if (zona.CostoAdicional < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.CostoAdicional),
                    "El costo adicional no puede ser negativo.");
            }

            if (zona.RadioMaximoKm.HasValue &&
                zona.RadioMaximoKm.Value <= 0)
            {
                ModelState.AddModelError(
                    nameof(zona.RadioMaximoKm),
                    "El radio máximo debe ser mayor que cero.");
            }

            if (zona.TarifaPorKm.HasValue &&
                zona.TarifaPorKm.Value < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.TarifaPorKm),
                    "La tarifa por kilómetro no puede ser negativa.");
            }

            if (!zona.LatitudCentro.HasValue ||
                !zona.LongitudCentro.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Debe seleccionar una ubicación en el mapa.");
            }
            else
            {
                if (zona.LatitudCentro.Value < -90 ||
                    zona.LatitudCentro.Value > 90)
                {
                    ModelState.AddModelError(
                        nameof(zona.LatitudCentro),
                        "La latitud seleccionada no es válida.");
                }

                if (zona.LongitudCentro.Value < -180 ||
                    zona.LongitudCentro.Value > 180)
                {
                    ModelState.AddModelError(
                        nameof(zona.LongitudCentro),
                        "La longitud seleccionada no es válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            zona.Nombre = zona.Nombre.Trim();

            _context.ZonasCobertura.Add(zona);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La zona de cobertura se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: AdminGeolocalizacion/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var zona = await _context.ZonasCobertura
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zona == null)
            {
                return NotFound();
            }

            return View(zona);
        }

        // POST: AdminGeolocalizacion/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            ZonaCobertura zona)
        {
            if (id != zona.IdZona)
            {
                return BadRequest();
            }
            if (!zona.LatitudCentro.HasValue ||
            !zona.LongitudCentro.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Debe seleccionar una ubicación en el mapa.");
            }
            else
            {
                if (zona.LatitudCentro.Value < -90 ||
                    zona.LatitudCentro.Value > 90)
                {
                    ModelState.AddModelError(
                        nameof(zona.LatitudCentro),
                        "La latitud seleccionada no es válida.");
                }

                if (zona.LongitudCentro.Value < -180 ||
                    zona.LongitudCentro.Value > 180)
                {
                    ModelState.AddModelError(
                        nameof(zona.LongitudCentro),
                        "La longitud seleccionada no es válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            bool nombreRepetido = await _context.ZonasCobertura
                .AnyAsync(z =>
                    z.IdZona != zona.IdZona &&
                    z.Nombre.ToLower() == zona.Nombre.ToLower());

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(zona.Nombre),
                    "Ya existe otra zona con ese nombre.");
            }

            if (zona.CostoAdicional < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.CostoAdicional),
                    "El costo adicional no puede ser negativo.");
            }

            if (zona.RadioMaximoKm.HasValue &&
                zona.RadioMaximoKm.Value <= 0)
            {
                ModelState.AddModelError(
                    nameof(zona.RadioMaximoKm),
                    "El radio máximo debe ser mayor que cero.");
            }

            if (zona.TarifaPorKm.HasValue &&
                zona.TarifaPorKm.Value < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.TarifaPorKm),
                    "La tarifa por kilómetro no puede ser negativa.");
            }

            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            var zonaExistente = await _context.ZonasCobertura
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zonaExistente == null)
            {
                return NotFound();
            }

            zonaExistente.Nombre = zona.Nombre.Trim();
            zonaExistente.CostoAdicional = zona.CostoAdicional;
            zonaExistente.LatitudCentro = zona.LatitudCentro;
            zonaExistente.LongitudCentro = zona.LongitudCentro;
            zonaExistente.RadioMaximoKm = zona.RadioMaximoKm;
            zonaExistente.TarifaPorKm = zona.TarifaPorKm;
            zonaExistente.Activa = zona.Activa;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La zona de cobertura se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // POST: AdminGeolocalizacion/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var zona = await _context.ZonasCobertura
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zona == null)
            {
                return NotFound();
            }

            zona.Activa = !zona.Activa;

            await _context.SaveChangesAsync();

            TempData["Success"] = zona.Activa
                ? "La zona fue activada correctamente."
                : "La zona fue desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: AdminGeolocalizacion/Detalles/5
        public async Task<IActionResult> Detalles(int id)
        {
            var zona = await _context.ZonasCobertura
                .Include(z => z.Ubicaciones)
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zona == null)
            {
                return NotFound();
            }

            return View(zona);
        }
    }
}