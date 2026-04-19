using System;
using System.Collections.Generic;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Services
{
    public interface IOfferService
    {
        IEnumerable<Offer> GetAll();
        Offer? GetById(Guid id);
        void Create(Offer offer);
        void Update(Offer offer);
        void Delete(Guid id);
        OfferValidationResult ValidateCode(string code, decimal total);
    }
}