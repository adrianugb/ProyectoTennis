using System;

namespace ProyectoGrupalTennis.Models
{
    public class Offer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty; // e.g. "SUMMER21"
        public string Description { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; } = 0m; // usa esto si > 0
        public decimal FixedDiscount { get; set; } = 0m; // o usa esto si > 0
        public bool IsActive { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}