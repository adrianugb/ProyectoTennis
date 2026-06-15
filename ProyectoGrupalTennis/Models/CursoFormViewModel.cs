using AcademiaTennisDAL.Entities;

namespace ProyectoGrupalTennis.Models
{
    public class CursoFormViewModel
    {
        public Curso Curso { get; set; } = new Curso();
        public List<Profesor> Profesores { get; set; } = new List<Profesor>();
        public List<Horario> Horarios { get; set; } = new List<Horario>();
        public Horario NuevoHorario { get; set; } = new Horario();
    }
}