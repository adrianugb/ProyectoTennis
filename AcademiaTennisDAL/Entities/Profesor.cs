using System;
using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class Profesor
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Nombre { get; set; }
        [Required, StringLength(100)]
        public string Apellidos { get; set; }
        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }
        [StringLength(50)]
        public string? Telefono { get; set; }
        [StringLength(200)]
        public string? Especialidad { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true; // ← nuevo
    }
}