using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class RachaAsistencia
    {
        [Key]
        public int IdRacha { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        public int RachaActual { get; set; } = 0;      // clases consecutivas asistidas

        public int RachaMaxima { get; set; } = 0;

        public int TotalClasesAsistidas { get; set; } = 0;

        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;
    }
}