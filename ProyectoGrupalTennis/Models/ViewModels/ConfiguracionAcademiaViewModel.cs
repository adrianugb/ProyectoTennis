using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models.ViewModels
{
    public class ConfiguracionAcademiaViewModel
    {
        [Required(ErrorMessage = "El número de WhatsApp es obligatorio.")]
        [RegularExpression(@"^\+?[1-9]\d{7,14}$",
            ErrorMessage = "Ingrese un número válido con código de país.")]
        [Display(Name = "Número de WhatsApp")]
        public string WhatsappSolicitudes { get; set; } = "";

        [Required(ErrorMessage = "El correo de contacto es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [MaxLength(256)]
        [Display(Name = "Correo de contacto")]
        public string CorreoSolicitudes { get; set; } = "";

        [Required(ErrorMessage = "El correo SMTP es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [MaxLength(256)]
        [Display(Name = "Correo remitente SMTP")]
        public string SmtpEmail { get; set; } = "";

        [Required]
        [MaxLength(200)]
        [Display(Name = "Servidor SMTP")]
        public string SmtpServer { get; set; } = "smtp.gmail.com";

        [Range(1, 65535)]
        [Display(Name = "Puerto SMTP")]
        public int SmtpPort { get; set; } = 587;

        [Required]
        [MaxLength(150)]
        [Display(Name = "Nombre del remitente")]
        public string SmtpSenderName { get; set; } = "Academia de Tennis";
    }
}