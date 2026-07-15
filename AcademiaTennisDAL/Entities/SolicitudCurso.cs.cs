using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class SolicitudCurso
    {
        [Key]
        public int IdSolicitudCurso { get; set; }

        [Required]
        public string IdAlumno { get; set; } = string.Empty;

        [ForeignKey(nameof(IdAlumno))]
        public ApplicationUser Alumno { get; set; } = null!;

        public int? IdCurso { get; set; }

        [ForeignKey(nameof(IdCurso))]
        public Curso? Curso { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCurso { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nivel { get; set; } = string.Empty;

        // Temporal: se conserva para no romper la funcionalidad actual.
        [Required]
        [StringLength(500)]
        public string Disponibilidad { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Modalidad { get; set; }
        // Individual / Pareja / Grupo

        public int? CantidadLecciones { get; set; }

        public bool RequiereEquipo { get; set; }

        public bool EsADomicilio { get; set; }

        [StringLength(500)]
        public string? DireccionDomicilio { get; set; }

        [StringLength(1000)]
        public string? Comentarios { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";
        // Pendiente / En revisión / Propuesta enviada /
        // Aceptada / Rechazada / Cancelada

        // Propuesta de la academia
        public DateTime? FechaPropuesta { get; set; }

        public TimeSpan? HoraInicioPropuesta { get; set; }

        public TimeSpan? HoraFinPropuesta { get; set; }

        public int? IdProfesorPropuesto { get; set; }

        [ForeignKey(nameof(IdProfesorPropuesto))]
        public Profesor? ProfesorPropuesto { get; set; }

        public int? IdCanchaPropuesta { get; set; }

        [ForeignKey(nameof(IdCanchaPropuesta))]
        public Cancha? CanchaPropuesta { get; set; }

        [StringLength(1000)]
        public string? ObservacionesAcademia { get; set; }

        public DateTime? FechaRespuestaAlumno { get; set; }

        // Nueva disponibilidad estructurada
        public ICollection<DisponibilidadSolicitud> Disponibilidades { get; set; }
            = new List<DisponibilidadSolicitud>();
    }
}