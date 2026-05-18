using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class TransaccionProducto
    {
        [Key]
        public int IdTransaccion { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [ForeignKey("IdProducto")]
        public Producto Producto { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } // Venta / Alquiler

        public int Cantidad { get; set; } = 1;

        public decimal MontoTotal { get; set; }

        public DateTime? FechaDevolucion { get; set; } // solo para alquileres

        public int? IdPago { get; set; }

        [ForeignKey("IdPago")]
        public Pago? Pago { get; set; }

        public DateTime FechaTransaccion { get; set; } = DateTime.Now;
    }
}