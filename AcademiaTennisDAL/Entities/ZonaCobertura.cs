using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class ZonaCobertura
    {
        [Key]
        public int IdZona { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoAdicional { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal? LatitudCentro { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal? LongitudCentro { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? RadioMaximoKm { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TarifaPorKm { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<UbicacionAlumno> Ubicaciones { get; set; }
            = new List<UbicacionAlumno>();
    }
}