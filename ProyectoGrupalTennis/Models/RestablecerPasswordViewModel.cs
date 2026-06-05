using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    public class RestablecerPasswordViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaPassword { get; set; }

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare("NuevaPassword",
            ErrorMessage = "La nueva contraseña y la confirmación no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarPassword { get; set; }
    }
}