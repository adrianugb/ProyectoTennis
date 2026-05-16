using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class Cancha
    {
        [Key]
        public int IdCancha { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        public bool Disponible { get; set; } = true;

        public bool EnMantenimiento { get; set; } = false;
    }
}