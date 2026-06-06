using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class ClaseProgramada
    {
        [Key]
        public int IdClaseProgramada { get; set; }

        public int IdCurso { get; set; }

        [ForeignKey("IdCurso")]
        public Curso Curso { get; set; }

        public DateTime FechaClase { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public string Estado { get; set; } = "Programada";
    }
}