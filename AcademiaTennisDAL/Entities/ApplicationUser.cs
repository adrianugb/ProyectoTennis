using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AcademiaTennisDAL.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        public bool Bloqueado { get; set; } = false;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}