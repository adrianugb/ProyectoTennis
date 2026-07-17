using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    public class SolicitudCursoViewModel
    {
        public int? IdCurso { get; set; }

        [Required]
        public string NombreCurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un nivel.")]
        public string Nivel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar el tipo de clase.")]
        public string TipoClase { get; set; } = string.Empty;
        // Individual / Pareja / Grupo

        [Required(ErrorMessage = "Debe indicar si la clase es con matrícula o sin matrícula.")]
        public string CondicionMatricula { get; set; } = string.Empty;
        // Con matrícula / Sin matrícula

        [Required(ErrorMessage = "Debe seleccionar una modalidad.")]
        public string Modalidad { get; set; } = string.Empty;
        // Academia / Domicilio

        [Required(ErrorMessage = "Debe seleccionar la cantidad de lecciones.")]
        [Range(1, 20, ErrorMessage = "La cantidad de lecciones no es válida.")]
        public int? CantidadLecciones { get; set; }

        [Range(1, 20, ErrorMessage = "La cantidad de personas no es válida.")]
        public int CantidadPersonas { get; set; } = 1;

        public bool RequiereEquipo { get; set; }

        public bool EsADomicilio { get; set; }

        [StringLength(500)]
        public string? DireccionDomicilio { get; set; }

        [Required(ErrorMessage = "Debe indicar su disponibilidad.")]
        [StringLength(500)]
        public string Disponibilidad { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Comentarios { get; set; }

        // Solo para mostrarlo al alumno.
        // El precio real debe calcularse nuevamente en el servidor.
        public decimal? PrecioEstimado { get; set; }

        public List<DisponibilidadSolicitudViewModel> Disponibilidades { get; set; }
            = new();
    }

    public class DisponibilidadSolicitudViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un día.")]
        public string DiaSemana { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar la hora inicial.")]
        public TimeSpan HoraDesde { get; set; }

        [Required(ErrorMessage = "Debe indicar la hora final.")]
        public TimeSpan HoraHasta { get; set; }
    }
}