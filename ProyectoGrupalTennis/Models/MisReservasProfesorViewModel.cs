namespace ProyectoGrupalTennis.Models
{
    public class MisReservasProfesorViewModel
    {
        public List<MiReservaProfesorItemViewModel> Reservas { get; set; } = new();
    }

    public class MiReservaProfesorItemViewModel
    {
        public int IdReserva { get; set; }

        public string Cancha { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Horario { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string? Alumno { get; set; }
    }
}