using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class ProgresoAlumno
    {
        [Key]
        public int IdProgreso { get; set; }

        [Required]
        public string IdAlumno { get; set; } = string.Empty;

        [ForeignKey(nameof(IdAlumno))]
        public ApplicationUser Alumno { get; set; } = null!;

        public string? IdProfesor { get; set; }

        [ForeignKey(nameof(IdProfesor))]
        public ApplicationUser? Profesor { get; set; }

        public int? IdCurso { get; set; }

        [ForeignKey(nameof(IdCurso))]
        public Curso? Curso { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de saque debe estar entre 1 y 10.")]
        public int NivelSaque { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de revés debe estar entre 1 y 10.")]
        public int NivelReves { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de derecha debe estar entre 1 y 10.")]
        public int NivelDerecha { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de volea debe estar entre 1 y 10.")]
        public int NivelVolea { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de movimiento debe estar entre 1 y 10.")]
        public int NivelMovimiento { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de táctica debe estar entre 1 y 10.")]
        public int NivelTactica { get; set; }

        [StringLength(1000)]
        public string? Observaciones { get; set; }

        [StringLength(1000)]
        public string? AreasMejora { get; set; }

        [Required]
        [StringLength(50)]
        public string NivelGeneral { get; set; } = "Principiante";

        public DateTime FechaEvaluacion { get; set; } = DateTime.Now;

        [NotMapped]
        public double Promedio =>
            (NivelSaque +
             NivelReves +
             NivelDerecha +
             NivelVolea +
             NivelMovimiento +
             NivelTactica) / 6.0;
    }
}