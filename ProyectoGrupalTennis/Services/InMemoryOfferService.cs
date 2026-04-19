using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Services
{
    public class InMemoryOfferService : IOfferService
    {
        private readonly ConcurrentDictionary<Guid, Offer> _store = new();

        public InMemoryOfferService()
        {
            // Ejemplos iniciales
            var o1 = new Offer
            {
                Code = "SUMMER21",
                Description = "Descuento de verano 15%",
                DiscountPercentage = 15m,
                IsActive = true,
                ValidFrom = DateTime.UtcNow.AddDays(-30),
                ValidTo = DateTime.UtcNow.AddDays(60)
            };

            var o2 = new Offer
            {
                Code = "WELCOME5",
                Description = "5 USD de descuento fijo",
                FixedDiscount = 5m,
                IsActive = true,
                ValidFrom = DateTime.UtcNow.AddDays(-10),
                ValidTo = DateTime.UtcNow.AddDays(365)
            };

            Create(o1);
            Create(o2);
        }

        public IEnumerable<Offer> GetAll() => _store.Values.OrderBy(o => o.Code);

        public Offer? GetById(Guid id) => _store.TryGetValue(id, out var o) ? o : null;

        public void Create(Offer offer)
        {
            if (offer.Id == Guid.Empty) offer.Id = Guid.NewGuid();
            _store[offer.Id] = offer;
        }

        public void Update(Offer offer)
        {
            if (offer.Id == Guid.Empty) return;
            _store[offer.Id] = offer;
        }

        public void Delete(Guid id)
        {
            _store.TryRemove(id, out _);
        }

        public OfferValidationResult ValidateCode(string code, decimal total)
        {
            var result = new OfferValidationResult
            {
                Success = false,
                Message = "Código inválido",
                DiscountAmount = 0m,
                NewTotal = total,
                AppliedCode = code
            };

            if (string.IsNullOrWhiteSpace(code))
            {
                result.Message = "Ingrese un código.";
                return result;
            }

            var offer = _store.Values.FirstOrDefault(o =>
                string.Equals(o.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));

            if (offer == null || !offer.IsActive)
            {
                result.Message = "Código no válido o inactivo.";
                return result;
            }

            var now = DateTime.UtcNow;
            if (offer.ValidFrom.HasValue && offer.ValidFrom.Value > now)
            {
                result.Message = "Código aún no válido.";
                return result;
            }

            if (offer.ValidTo.HasValue && offer.ValidTo.Value < now)
            {
                result.Message = "Código expirado.";
                return result;
            }

            decimal discount = 0m;
            if (offer.DiscountPercentage > 0)
            {
                discount = Math.Round(total * (offer.DiscountPercentage / 100m), 2);
            }
            else if (offer.FixedDiscount > 0)
            {
                discount = Math.Min(offer.FixedDiscount, total);
            }

            var newTotal = Math.Max(0m, total - discount);

            result.Success = true;
            result.Message = $"Código aplicado: {offer.Code}. Descuento: {discount:C2}";
            result.DiscountAmount = discount;
            result.NewTotal = newTotal;
            return result;
        }
    }
}