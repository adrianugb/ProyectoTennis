using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public string Nivel { get; set; } = string.Empty;

        public int CuposDisponibles { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;

        // FK Profesor
        public int? IdProfesor { get; set; }

        [ForeignKey("IdProfesor")]
        public Profesor? Profesor { get; set; }



        public ICollection<Horario> Horarios { get; set; } = new List<Horario>();
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}