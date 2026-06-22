namespace ProyectoGrupalTennis.Models
{
    public class AdminReservasViewModel
    {
        public string? MensajeExito { get; set; }

        public string? MensajeError { get; set; }

        public List<AdminReservaItemViewModel> Reservas { get; set; }
            = new();
    }

    public class AdminReservaItemViewModel
    {
        public int IdReserva { get; set; }

        public string Cancha { get; set; } = string.Empty;

        public string Profesor { get; set; } = string.Empty;

        public string Alumno { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Horario { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
    }
}