using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Notificacion
    {
        [Key]
        public int IdNotificacion { get; set; }

        [Required]
        public string IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public ApplicationUser Usuario { get; set; }

        [Required]
        [StringLength(100)]
        public string Tipo { get; set; }
        // Reserva / Pago / Recordatorio / Cancelacion

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(1000)]
        public string Mensaje { get; set; }

        public bool Leida { get; set; } = false;

        public DateTime FechaEnvio { get; set; } = DateTime.Now;
    }
}