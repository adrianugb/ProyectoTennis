using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class ZonaCobertura
    {
        [Key]
        public int IdZona { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } // Ej: "Escazú", "San José Centro"

        [Required]
        public decimal CostoAdicional { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<UbicacionAlumno> Ubicaciones { get; set; }
    }
}