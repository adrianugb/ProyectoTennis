using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Horario
    {
        [Key]
        public int IdHorario { get; set; }

        [Required]
        public string DiaSemana { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        public int IdCurso { get; set; }

        [ForeignKey("IdCurso")]
        public Curso? Curso { get; set; }
    }
}