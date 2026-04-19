namespace ProyectoGrupalTennis.Models
{
    public class OfferValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal NewTotal { get; set; }
        public string AppliedCode { get; set; } = string.Empty;
    }
}