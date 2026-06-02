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
    }
}