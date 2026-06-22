namespace ProyectoGrupalTennis.Models
{
    public class ClasesDisponiblesViewModel
    {
        public List<ClaseDisponibleItemViewModel> Clases { get; set; } = new();
    }

    public class ClaseDisponibleItemViewModel
    {
        public int IdReserva { get; set; }

        public string Profesor { get; set; } = string.Empty;

        public string Cancha { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Horario { get; set; } = string.Empty;
    }
}