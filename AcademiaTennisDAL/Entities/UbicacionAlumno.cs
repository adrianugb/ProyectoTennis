using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class UbicacionAlumno
    {
        [Key]
        public int IdUbicacion { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        [Required]
        [StringLength(300)]
        public string DireccionCompleta { get; set; }

        public decimal Latitud { get; set; }

        public decimal Longitud { get; set; }

        public int? IdZona { get; set; }

        [ForeignKey("IdZona")]
        public ZonaCobertura? Zona { get; set; }

        public bool EsPrincipal { get; set; } = true;
    }
}