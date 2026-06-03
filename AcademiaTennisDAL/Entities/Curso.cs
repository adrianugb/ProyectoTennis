using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AcademiaTennisDAL.Entities;

namespace AcademiaTennisDAL.Entities
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public string Nivel { get; set; } = string.Empty;

        public int CuposDisponibles { get; set; }

        public bool Activo { get; set; } = true;

        // FK al usuario con rol Profesor (ADM-04-006)
        public string? IdProfesorUserId { get; set; }

        [ForeignKey("IdProfesorUserId")]
        public ApplicationUser? Profesor { get; set; }

        public ICollection<Horario> Horarios { get; set; } = new List<Horario>();
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}