using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class UbicacionAlumno
    {
        [Key]
        public int IdUbicacion { get; set; }

        [Required]
        public string IdAlumno { get; set; } = string.Empty;

        [ForeignKey(nameof(IdAlumno))]
        public ApplicationUser Alumno { get; set; } = null!;

        [Required]
        [StringLength(300)]
        public string DireccionCompleta { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitud { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitud { get; set; }

        public int? IdZona { get; set; }

        [ForeignKey(nameof(IdZona))]
        public ZonaCobertura? Zona { get; set; }

        public bool EsPrincipal { get; set; } = true;
    }
}