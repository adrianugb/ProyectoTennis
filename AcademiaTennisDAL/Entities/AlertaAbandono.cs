using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class AlertaAbandono
    {
        [Key]
        public int IdAlerta { get; set; }

        [Required]
        public string IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser Alumno { get; set; }

        [Required]
        [StringLength(300)]
        public string Motivo { get; set; } // Ej: "3 clases sin asistir", "2 cancelaciones seguidas"

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente / Gestionada / Resuelta

        public DateTime FechaAlerta { get; set; } = DateTime.Now;

        public DateTime? FechaGestion { get; set; }

        [StringLength(500)]
        public string? AccionTomada { get; set; }
    }
}