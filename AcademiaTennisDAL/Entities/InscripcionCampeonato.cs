using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class InscripcionCampeonato
    {
        [Key]
        public int IdInscripcion { get; set; }

        [Required]
        public int IdCampeonato { get; set; }

        [ForeignKey("IdCampeonato")]
        public Campeonato Campeonato { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Estado { get; set; } = "Activa"; // Activa / Eliminado / Ganador
    }
}