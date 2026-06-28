using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    public class SolicitudCursoViewModel
    {
        public int? IdCurso { get; set; }

        public string NombreCurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un nivel.")]
        public string Nivel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar su disponibilidad.")]
        public string Disponibilidad { get; set; } = string.Empty;

        public string? Comentarios { get; set; }
    }
}