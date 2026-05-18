using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class EncuestaSatisfaccion
    {
        [Key]
        public int IdEncuesta { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        [Required]
        public int IdReserva { get; set; }

        [ForeignKey("IdReserva")]
        public Reserva Reserva { get; set; }

        public int CalificacionClase { get; set; }       // 1-5

        public int CalificacionProfesor { get; set; }    // 1-5

        public int CalificacionInstalaciones { get; set; } // 1-5

        [StringLength(1000)]
        public string? Comentarios { get; set; }

        [StringLength(500)]
        public string? Recomendaciones { get; set; }

        public DateTime FechaEncuesta { get; set; } = DateTime.Now;
    }
}