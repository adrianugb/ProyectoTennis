using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class PreguntaFrecuente
    {
        [Key]
        public int IdPregunta { get; set; }

        [Required]
        [StringLength(300)]
        public string Pregunta { get; set; }

        [Required]
        [StringLength(2000)]
        public string Respuesta { get; set; }

        [StringLength(50)]
        public string Categoria { get; set; } // Cursos / Horarios / Reservas / Pagos / General

        public bool Activa { get; set; } = true;

        public int VecesConsultada { get; set; } = 0;
    }
}