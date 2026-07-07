using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Matricula
    {
        [Key]
        public int IdMatricula { get; set; }

        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        public int IdCurso { get; set; }

        [ForeignKey("IdCurso")]
        public Curso Curso { get; set; }

        public DateTime FechaMatricula { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Activa";

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}