using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public string IdAlumno { get; set; } = string.Empty;

        [ForeignKey(nameof(IdAlumno))]
        public ApplicationUser Alumno { get; set; } = null!;

        [Required]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoPago { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaPago { get; set; } = DateTime.Now;

        public DateTime? FechaVencimiento { get; set; }

        public bool EsManual { get; set; } = false;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public int? IdMatricula { get; set; }

        [ForeignKey(nameof(IdMatricula))]
        public Matricula? Matricula { get; set; }

        public int? IdReserva { get; set; }

        [ForeignKey(nameof(IdReserva))]
        public Reserva? Reserva { get; set; }

        public Factura? Factura { get; set; }

        public int? IdCurso { get; set; }

        [ForeignKey(nameof(IdCurso))]
        public Curso? Curso { get; set; }

        [StringLength(300)]
        public string? ComprobantePago { get; set; }

        public DateTime? FechaComprobante { get; set; }
    }
}