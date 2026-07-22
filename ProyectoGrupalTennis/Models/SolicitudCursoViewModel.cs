using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    public class SolicitudCursoViewModel
    {
        public int? IdCurso { get; set; }

        [Required(
            ErrorMessage = "Debe seleccionar una tarifa o paquete."
        )]
        public int? IdTarifaClase { get; set; }

        // Datos informativos de la tarifa seleccionada.
        // No se confía en ellos para guardar el precio real.
        public string NombreCurso { get; set; } = string.Empty;

        public string TipoClase { get; set; } = string.Empty;

        public string CondicionMatricula { get; set; } = string.Empty;

        public int CantidadLecciones { get; set; }

        public bool PrecioPorPersona { get; set; }

        public decimal? PrecioEstimado { get; set; }

        [Required(
            ErrorMessage = "Debe seleccionar un nivel."
        )]
        public string Nivel { get; set; } = string.Empty;

        [Range(
            1,
            20,
            ErrorMessage = "La cantidad de personas no es válida."
        )]
        public int CantidadPersonas { get; set; } = 1;

        public bool RequiereEquipo { get; set; }

        public bool EsADomicilio { get; set; }

        [StringLength(500)]
        public string? DireccionDomicilio { get; set; }

        [StringLength(1000)]
        public string? Comentarios { get; set; }

        public List<DisponibilidadSolicitudViewModel>
            Disponibilidades
        { get; set; } = new();
    }

    public class DisponibilidadSolicitudViewModel
    {
        [Required(
            ErrorMessage = "Debe seleccionar un día."
        )]
        public string DiaSemana { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Debe indicar la hora inicial."
        )]
        public TimeSpan HoraDesde { get; set; }

        [Required(
            ErrorMessage = "Debe indicar la hora final."
        )]
        public TimeSpan HoraHasta { get; set; }
    }
}