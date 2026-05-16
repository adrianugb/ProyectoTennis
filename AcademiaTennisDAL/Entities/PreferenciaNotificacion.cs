using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class PreferenciaNotificacion
    {
        [Key]
        public int IdPreferencia { get; set; }

        [Required]
        public string IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public ApplicationUser Usuario { get; set; }

        [Required]
        [StringLength(50)]
        public string CanalPreferido { get; set; }
        // Email / SMS / Push / WhatsApp

        public bool NotificacionesPago { get; set; } = true;

        public bool NotificacionesClase { get; set; } = true;

        public bool NotificacionesRecordatorio { get; set; } = true;

        public bool NotificacionesCampeonato { get; set; } = true;
    }
}