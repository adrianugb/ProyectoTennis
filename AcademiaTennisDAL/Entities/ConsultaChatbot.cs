using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class ConsultaChatbot
    {
        [Key]
        public int IdConsulta { get; set; }

        public string? IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public ApplicationUser? Usuario { get; set; }

        [Required]
        [StringLength(500)]
        public string MensajeUsuario { get; set; }

        [StringLength(2000)]
        public string? RespuestaBot { get; set; }

        public DateTime FechaConsulta { get; set; } = DateTime.Now;

        public bool Resuelto { get; set; } = false;
    }
}