using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Reserva
    {
        [Key]
        public int IdReserva { get; set; }

        public int IdCancha { get; set; }

        [ForeignKey("IdCancha")]
        public Cancha Cancha { get; set; }

        public string IdProfesor { get; set; }

        [ForeignKey("IdProfesor")]
        public ApplicationUser Profesor { get; set; }

        public string? IdAlumno { get; set; }

        [ForeignKey("IdAlumno")]
        public ApplicationUser? Alumno { get; set; }

        public DateTime FechaReserva { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        public string Estado { get; set; } = "Disponible";

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}