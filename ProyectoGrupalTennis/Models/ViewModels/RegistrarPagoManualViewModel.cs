using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;


namespace ProyectoGrupalTennis.Models
{
    public class RegistrarPagoManualViewModel
    {
        [Required]
        public string IdAlumno { get; set; } = string.Empty;

        [Required]
        public string TipoPago { get; set; } = string.Empty;

        [Required]
        public decimal Monto { get; set; }

        [Required]
        public string MetodoPago { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public List<SelectListItem> Alumnos { get; set; } = new();
    }
}