using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models.ViewModels
{
    // ADM-09-002: modelo para el formulario "Notificar a grupos de alumnos"
    public class NotificarGrupoViewModel
    {
        // Grupo destino: el curso seleccionado. Si queda en null, la notificacion
        // se envia a todos los alumnos con matricula activa en cualquier curso.
        public int? IdCurso { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede superar los 1000 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        // Cursos activos disponibles para elegir como grupo destino
        public List<SelectListItem> Cursos { get; set; } = new();

        // Resultado tras el envio: cuantos alumnos recibieron la notificacion
        public int? AlumnosNotificados { get; set; }
    }
}
