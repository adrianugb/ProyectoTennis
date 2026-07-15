using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class DisponibilidadSolicitud
    {
        [Key]
        public int IdDisponibilidadSolicitud { get; set; }

        public int IdSolicitudCurso { get; set; }

        [ForeignKey(nameof(IdSolicitudCurso))]
        public SolicitudCurso SolicitudCurso { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string DiaSemana { get; set; } = string.Empty;

        [Required]
        public TimeSpan HoraDesde { get; set; }

        [Required]
        public TimeSpan HoraHasta { get; set; }
    }
}