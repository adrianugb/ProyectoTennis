using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class Campeonato
    {
        [Key]
        public int IdCampeonato { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(500)]
        public string? Reglas { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public int MaxParticipantes { get; set; }

        [StringLength(30)]
        public string Estado { get; set; } = "Abierto"; // Abierto / EnCurso / Finalizado / Cancelado

        public ICollection<InscripcionCampeonato> Inscripciones { get; set; }
        public ICollection<Enfrentamiento> Enfrentamientos { get; set; }
    }
}