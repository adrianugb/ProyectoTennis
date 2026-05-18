using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public decimal? PrecioAlquiler { get; set; }

        [Required]
        public int Stock { get; set; }

        [StringLength(50)]
        public string Categoria { get; set; } // Raqueta / Bola / Cuerda / Accesorio

        public bool Activo { get; set; } = true;

        public ICollection<TransaccionProducto> Transacciones { get; set; }
    }
}