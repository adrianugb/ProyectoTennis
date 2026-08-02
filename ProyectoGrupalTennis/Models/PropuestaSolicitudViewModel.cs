using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    public class PropuestaSolicitudViewModel
    {
        public int IdSolicitudCurso { get; set; }

        public string CodigoSolicitud { get; set; } = string.Empty;

        public string NombreAlumno { get; set; } = string.Empty;

        public string NombreCurso { get; set; } = string.Empty;

        public string Nivel { get; set; } = string.Empty;

        public string DisponibilidadAlumno { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string? MotivoRechazoAlumno { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha.")]
        [DataType(DataType.Date)]
        public DateTime? FechaPropuesta { get; set; }

        [Required(ErrorMessage = "Debe indicar la hora de inicio.")]
        [DataType(DataType.Time)]
        public TimeSpan? HoraInicioPropuesta { get; set; }

        [Required(ErrorMessage = "Debe indicar la hora de finalización.")]
        [DataType(DataType.Time)]
        public TimeSpan? HoraFinPropuesta { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un profesor.")]
        public int? IdProfesorPropuesto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cancha.")]
        public int? IdCanchaPropuesta { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "Las observaciones no pueden superar los 1000 caracteres.")]
        public string? ObservacionesAcademia { get; set; }
    }
}