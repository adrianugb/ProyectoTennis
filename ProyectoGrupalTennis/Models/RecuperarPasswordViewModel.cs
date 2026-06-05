using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    public class RecuperarPasswordViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }
}