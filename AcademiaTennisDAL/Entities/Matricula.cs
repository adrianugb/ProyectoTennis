using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Matricula
    {
        [Key]
        public int IdMatricula { get; set; }

        [Required]
        public string IdAlumno { get; set; } = string.Empty;

        [ForeignKey(nameof(IdAlumno))]
        public ApplicationUser Alumno { get; set; } = null!;

        public int IdCurso { get; set; }

        [ForeignKey(nameof(IdCurso))]
        public Curso Curso { get; set; } = null!;

        public DateTime FechaMatricula { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Activa";

        public bool EsADomicilio { get; set; } = false;

        public int? IdUbicacionAlumno { get; set; }

        [ForeignKey(nameof(IdUbicacionAlumno))]
        public UbicacionAlumno? UbicacionAlumno { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DistanciaKm { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoDesplazamiento { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCurso { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoTotal { get; set; }

        public ICollection<Pago> Pagos { get; set; }
            = new List<Pago>();
    }
}