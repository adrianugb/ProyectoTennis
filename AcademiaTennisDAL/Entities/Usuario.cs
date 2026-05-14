using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [Required]
    [StringLength(100)]
    public string Apellido { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Correo { get; set; }

    [Required]
    public string Contrasena { get; set; }

    public bool Activo { get; set; } = true;

    public bool Bloqueado { get; set; } = false;

    [Required]
    public int IdRol { get; set; }

    [ForeignKey("IdRol")]
    public Rol Rol { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}