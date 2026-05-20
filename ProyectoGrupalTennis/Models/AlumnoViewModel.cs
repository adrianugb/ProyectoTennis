using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{

    // ViewModel para mostrar la lista de alumnos en la vista AdminAlumnos.
    public class AlumnoListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    // ViewModel para crear un alumno nuevo
    public class CrearAlumnoViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña temporal")]
        public string Contrasena { get; set; } = string.Empty;
    }


    // ViewModel para editar un alumno existente

    public class EditarAlumnoViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;
    }

    // ViewModel principal que la vista AdminAlumnos recibe como modelo.

    public class AdminAlumnosViewModel
    {
        public IList<AlumnoListItemViewModel> Alumnos { get; set; } = new List<AlumnoListItemViewModel>();
        public CrearAlumnoViewModel NuevoAlumno { get; set; } = new CrearAlumnoViewModel();
        public EditarAlumnoViewModel? AlumnoAEditar { get; set; }

        // Mensajes de resultado para mostrar en la vista
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }
    }
}