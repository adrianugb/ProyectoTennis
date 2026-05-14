using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class CuponUso
    {
        [Key]
        public int IdUso { get; set; }

        [Required]
        public int IdCupon { get; set; }

        [ForeignKey("IdCupon")]
        public Cupon Cupon { get; set; }

        [Required]
        public string IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public ApplicationUser Usuario { get; set; }

        public DateTime FechaUso { get; set; } = DateTime.Now;
    }
}