using Microsoft.AspNetCore.Mvc;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;
using System;

namespace ProyectoGrupalTennis.Controllers
{
    public class OffersController : Controller
    {
        private readonly IOfferService _offerService;

       
        public OffersController(IOfferService? offerService = null)
        {
            _offerService = offerService ?? new InMemoryOfferService();
        }

        // Módulo administración - Listado
        public IActionResult Index()
        {
            var items = _offerService.GetAll();
            return View("~/Views/Offers/AdminIndex.cshtml", items);
        }

        // Crear
        public IActionResult Create()
        {
            return View("~/Views/Offers/Create.cshtml", new Offer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Offer model)
        {
            if (!ModelState.IsValid) return View("~/Views/Offers/Create.cshtml", model);

            _offerService.Create(model);
            return RedirectToAction(nameof(Index));
        }

        // Editar
        public IActionResult Edit(Guid id)
        {
            var item = _offerService.GetById(id);
            if (item == null) return NotFound();
            return View("~/Views/Offers/Edit.cshtml", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Offer model)
        {
            if (!ModelState.IsValid) return View("~/Views/Offers/Edit.cshtml", model);

            _offerService.Update(model);
            return RedirectToAction(nameof(Index));
        }

        // Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            _offerService.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        // Endpoint para que usuarios apliquen cupones desde la tienda (AJAX)
        [HttpPost]
        public IActionResult ApplyCoupon([FromForm] string code, [FromForm] decimal price)
        {
            var res = _offerService.ValidateCode(code ?? string.Empty, price);
            return Json(res);
        }
    }
}