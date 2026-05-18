using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcademiaTennisDAL.Entities
{
    public class Enfrentamiento
    {
        [Key]
        public int IdEnfrentamiento { get; set; }

        [Required]
        public int IdCampeonato { get; set; }

        [ForeignKey("IdCampeonato")]
        public Campeonato Campeonato { get; set; }

        [Required]
        public string IdJugador1 { get; set; }

        [ForeignKey("IdJugador1")]
        public ApplicationUser Jugador1 { get; set; }

        [Required]
        public string IdJugador2 { get; set; }

        [ForeignKey("IdJugador2")]
        public ApplicationUser Jugador2 { get; set; }

        public string? IdGanador { get; set; }

        [ForeignKey("IdGanador")]
        public ApplicationUser? Ganador { get; set; }

        [StringLength(50)]
        public string? Resultado { get; set; } // Ej: "6-3, 7-5"

        public DateTime? FechaPartido { get; set; }

        public int? IdCancha { get; set; }

        [ForeignKey("IdCancha")]
        public Cancha? Cancha { get; set; }
    }
}