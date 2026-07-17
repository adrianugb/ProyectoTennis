using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class TipoClase
    {
        [Key]
        public int IdTipoClase { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de clase es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        public ICollection<TarifaClase> TarifasClase { get; set; }
            = new List<TarifaClase>();
    }
}