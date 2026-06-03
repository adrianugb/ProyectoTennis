using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models
{
    // ── ADM-04-001 / ADM-04-002: Admin gestiona canchas ──

    public class CanchaItemViewModel
    {
        public int IdCancha { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        public bool EnMantenimiento { get; set; }

        public string EstadoTexto =>
            EnMantenimiento ? "En mantenimiento" :
            Disponible ? "Disponible" : "No disponible";
    }

    public class NuevaCanchaViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre de la cancha")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;
    }

    public class EditarCanchaViewModel
    {
        public int IdCancha { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre de la cancha")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; }
    }

    public class AdminCanchasViewModel
    {
        public IList<CanchaItemViewModel> Canchas { get; set; } = new List<CanchaItemViewModel>();
        public NuevaCanchaViewModel NuevaCancha { get; set; } = new NuevaCanchaViewModel();
        public EditarCanchaViewModel? CanchaAEditar { get; set; }
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }
        public string? FiltroBuscar { get; set; }
        public string? FiltroEstado { get; set; }
    }
}