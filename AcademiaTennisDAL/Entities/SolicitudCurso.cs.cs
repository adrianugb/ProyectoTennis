using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class SolicitudCurso
    {
        [Key]
        public int IdSolicitudCurso { get; set; }

        public string IdAlumno { get; set; } = string.Empty;

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; } = null!;

        public int? IdCurso { get; set; }

        [ForeignKey("IdCurso")]
        public Curso? Curso { get; set; }

        [Required]
        public string NombreCurso { get; set; } = string.Empty;

        [Required]
        public string Nivel { get; set; } = string.Empty;

        [Required]
        public string Disponibilidad { get; set; } = string.Empty;

        public string? Comentarios { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";
    }
}