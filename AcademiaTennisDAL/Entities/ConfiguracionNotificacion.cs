using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class ConfiguracionNotificacion
    {
        [Key]
        public int IdConfiguracion { get; set; }

        [Required]
        [StringLength(100)]
        public string Evento { get; set; }

        public bool Activo { get; set; } = true;
    }
}