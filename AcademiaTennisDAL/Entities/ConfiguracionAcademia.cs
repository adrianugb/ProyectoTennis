using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class ConfiguracionAcademia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string WhatsappSolicitudes { get; set; } = "";

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string CorreoSolicitudes { get; set; } = "";

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string SmtpEmail { get; set; } = "";

        [Required]
        [MaxLength(200)]
        public string SmtpServer { get; set; } = "smtp.gmail.com";

        public int SmtpPort { get; set; } = 587;

        [MaxLength(150)]
        public string SmtpSenderName { get; set; }
            = "Academia de Tennis";

        public DateTime FechaActualizacion { get; set; }
            = DateTime.UtcNow;
    }
}