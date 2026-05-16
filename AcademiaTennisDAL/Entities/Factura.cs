using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Factura
    {
        [Key]
        public int IdFactura { get; set; }

        [Required]
        public int IdPago { get; set; }

        [ForeignKey("IdPago")]
        public Pago Pago { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroFactura { get; set; }

        public DateTime FechaFactura { get; set; } = DateTime.Now;
    }
}