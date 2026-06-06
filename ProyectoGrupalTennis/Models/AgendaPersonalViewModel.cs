namespace ProyectoGrupalTennis.Models
{
    public class AgendaPersonalViewModel
    {
        public string? FiltroDia { get; set; }

        public List<string> DiasDisponibles { get; set; } = new();

        public string? FiltroFecha { get; set; }


        public List<AgendaPersonalItemViewModel> Clases { get; set; } = new();
    }

    public class AgendaPersonalItemViewModel
    {
        public string Curso { get; set; } = string.Empty;

        public string Nivel { get; set; } = string.Empty;

        public string DiaSemana { get; set; } = string.Empty;
        public string FechaClase { get; set; } = string.Empty;


        public string HoraInicio { get; set; } = string.Empty;

        public string HoraFin { get; set; } = string.Empty;

        public string EstadoMatricula { get; set; } = string.Empty;
    }
}