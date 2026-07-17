using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class CondicionServicio
    {
        [Key]
        public int IdCondicionServicio { get; set; }

        [Required]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        public int Orden { get; set; }

        public bool Activa { get; set; } = true;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}