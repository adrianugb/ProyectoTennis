using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public string Nivel { get; set; }

        public int CuposDisponibles { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Horario> Horarios { get; set; }

        public ICollection<Matricula> Matriculas { get; set; }
    }
}