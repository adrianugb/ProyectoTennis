using AcademiaTennisDAL.Entities;

namespace AcademiaTennisDAL.Repositories
{
    public interface ICursoRepository
    {
        List<Curso> ObtenerTodos();
        Curso? ObtenerPorId(int id);
        void Agregar(Curso curso);
        void Actualizar(Curso curso);
        void CambiarEstado(int id, bool activo);
        List<Profesor> ObtenerProfesores();
        //Horarios
        List<Horario> ObtenerHorarios(int idCurso);
        void AgregarHorario(Horario horario);
        void EliminarHorario(int idHorario);
    }
}