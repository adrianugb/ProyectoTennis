using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class ProgresoAlumno
    {
        [Key]
        public int IdProgreso { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        public string? IdProfesor { get; set; }

        [ForeignKey("IdProfesor")]
        public ApplicationUser? Profesor { get; set; }

        // Métricas 1-10
        public int NivelSaque { get; set; }
        public int NivelReves { get; set; }
        public int NivelDerecha { get; set; }
        public int NivelVolea { get; set; }
        public int NivelMovimiento { get; set; }
        public int NivelTactica { get; set; }

        [StringLength(1000)]
        public string? Observaciones { get; set; }

        [StringLength(50)]
        public string NivelGeneral { get; set; } = "Iniciante"; // Iniciante / Intermedio / Avanzado

        public DateTime FechaEvaluacion { get; set; } = DateTime.Now;
    }
}