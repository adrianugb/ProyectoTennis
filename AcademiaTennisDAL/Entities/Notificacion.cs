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

        // USER-09-008: canal por el que finalmente quedo disponible la notificacion
        // ("Email" o "Plataforma"). Siempre queda un valor, ya que la notificacion
        // siempre se guarda dentro de la app como respaldo.
        [StringLength(50)]
        public string CanalUsado { get; set; } = "Plataforma";

        // USER-09-008: true cuando el canal preferido del alumno (por ejemplo Email)
        // no pudo entregarse y el sistema tuvo que dejar constancia del error,
        // usando la plataforma como medio alterno.
        public bool EnvioFallido { get; set; } = false;
    }
}