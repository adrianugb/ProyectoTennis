using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    public class ReservarCanchaViewModel
    {
        [Required]
        [Display(Name = "Cancha")]
        public int IdCancha { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime FechaReserva { get; set; }

        [Required]
        [Display(Name = "Hora Inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        [Display(Name = "Hora Fin")]
        public TimeSpan HoraFin { get; set; }

        public List<SelectListItem> Canchas { get; set; } = new();

        public string? MensajeExito { get; set; }

        public string? MensajeError { get; set; }
    }
}