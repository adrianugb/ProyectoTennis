using AcademiaTennisDAL.Entities;

namespace ProyectoGrupalTennis.Models
{
    public class CursoFormViewModel
    {
        public Curso Curso { get; set; } = new Curso();
        public List<Profesor> Profesores { get; set; } = new List<Profesor>();
        public List<HorarioInputViewModel> Horarios { get; set; } = new List<HorarioInputViewModel>();
        public Horario NuevoHorario { get; set; } = new Horario();
    }

    public class HorarioInputViewModel
    {
        public int IdHorario { get; set; }
        public string Fecha { get; set; } = string.Empty;      // "yyyy-MM-dd"
        public string HoraInicio { get; set; } = string.Empty; // "HH:mm"
        public string HoraFin { get; set; } = string.Empty;    // "HH:mm"

        // Para mostrar en tabla
        public string DiaSemana => string.IsNullOrWhiteSpace(Fecha)
            ? string.Empty
            : DateTime.Parse(Fecha).ToString("dddd", new System.Globalization.CultureInfo("es-ES"));
    }
}