using AcademiaTennisDAL.Entities;

namespace AcademiaTennisDAL.Repositories
{
    public interface ICursoRepository
    {
        List<Curso> ObtenerTodos();
        Curso? ObtenerPorId(int id);
        void Agregar(Curso Curso, List<Horario> horarios);
        void Actualizar(Curso Curso, List<Horario> horarios);
        void CambiarEstado(int id, bool activo);
        List<Profesor> ObtenerProfesores();
    }
}