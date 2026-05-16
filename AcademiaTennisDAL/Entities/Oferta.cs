using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace AcademiaTennisDAL.Entities
{
    public class Oferta
    {
        [Key]
        public int IdOferta { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public decimal Descuento { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public bool Activa { get; set; } = true;

        [StringLength(200)]
        public string ?ProductosAplicables { get; set; }

        public ICollection<Cupon> Cupones { get; set; }
    }
}