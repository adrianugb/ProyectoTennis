using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        [Required]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoPago { get; set; }
        // Reserva / Matricula

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; }
        // Tarjeta / Sinpe / Efectivo

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";
        // Pendiente / Pagado / Anulado / Rechazado

        public DateTime FechaPago { get; set; } = DateTime.Now;

        public bool EsManual { get; set; } = false;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public Factura? Factura { get; set; }
    }
}