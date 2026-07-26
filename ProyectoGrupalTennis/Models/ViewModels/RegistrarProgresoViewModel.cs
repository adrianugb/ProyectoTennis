using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProyectoGrupalTennis.Models
{
    public class RegistrarProgresoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un alumno.")]
        [Display(Name = "Alumno")]
        public string IdAlumno { get; set; } = string.Empty;

        [Display(Name = "Curso")]
        public int? IdCurso { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "El nivel debe estar entre 1 y 10.")]
        [Display(Name = "Saque")]
        public int NivelSaque { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "El nivel debe estar entre 1 y 10.")]
        [Display(Name = "Revés")]
        public int NivelReves { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "El nivel debe estar entre 1 y 10.")]
        [Display(Name = "Derecha")]
        public int NivelDerecha { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "El nivel debe estar entre 1 y 10.")]
        [Display(Name = "Volea")]
        public int NivelVolea { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "El nivel debe estar entre 1 y 10.")]
        [Display(Name = "Movimiento")]
        public int NivelMovimiento { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "El nivel debe estar entre 1 y 10.")]
        [Display(Name = "Táctica")]
        public int NivelTactica { get; set; }

        [StringLength(1000)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [StringLength(1000)]
        [Display(Name = "Áreas de mejora")]
        public string? AreasMejora { get; set; }

        public List<SelectListItem> Alumnos { get; set; } = new();

        public List<SelectListItem> Cursos { get; set; } = new();
    }
}