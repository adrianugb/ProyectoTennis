using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class TarifaClase
    {
        [Key]
        public int IdTarifaClase { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // FK -> TipoClase
        [Required]
        public int IdTipoClase { get; set; }

        [ForeignKey(nameof(IdTipoClase))]
        public TipoClase TipoClase { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string CondicionMatricula { get; set; } = string.Empty;
        // Con matrícula / Sin matrícula

        [Required]
        public int CantidadLecciones { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public bool PrecioPorPersona { get; set; }

        [StringLength(300)]
        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}