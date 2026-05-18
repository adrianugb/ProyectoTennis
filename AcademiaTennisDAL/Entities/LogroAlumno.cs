using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class LogroAlumno
    {
        [Key]
        public int IdLogro { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoLogro { get; set; } // Racha5Dias / PrimerCampeonato / 50Clases etc.

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public DateTime FechaObtencion { get; set; } = DateTime.Now;
    }
}