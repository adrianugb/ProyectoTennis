using AcademiaTennisDAL.Entities;

namespace ProyectoGrupalTennis.Models
{
    public class HorarioInputViewModel
    {
        public int IdHorario { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty; // "HH:mm"
        public string HoraFin { get; set; } = string.Empty;
    }

    public class CursoFormViewModel
    {
        public Curso Curso { get; set; } = new Curso();
        public List<Profesor> Profesores { get; set; } = new List<Profesor>();
        public List<HorarioInputViewModel> Horarios { get; set; } = new List<HorarioInputViewModel>();

    }
}