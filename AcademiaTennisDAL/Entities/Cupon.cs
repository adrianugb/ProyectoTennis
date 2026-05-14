using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Cupon
    {
        [Key]
        public int IdCupon { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; }

        [Required]
        public decimal Descuento { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public int LimiteUsos { get; set; }

        public int UsosActuales { get; set; } = 0;

        public bool Activo { get; set; } = true;

        public int? IdOferta { get; set; }

        [ForeignKey("IdOferta")]
        public Oferta Oferta { get; set; }

        public ICollection<CuponUso> Usos { get; set; }
    }
}