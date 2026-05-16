using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class AsistenciaClase
    {
        [Key]
        public int IdAsistencia { get; set; }

        public int IdReserva { get; set; }

        [ForeignKey("IdReserva")]
        public Reserva Reserva { get; set; }

        public bool Asistio { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}