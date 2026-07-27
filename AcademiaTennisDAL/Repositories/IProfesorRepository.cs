using AcademiaTennisDAL.Entities;

namespace AcademiaTennisDAL.Repositories
{
    public interface IProfesorRepository
    {

        List<Profesor> ObtenerTodos();
        Profesor? ObtenerPorId(int id);
        void Agregar(Profesor profesor);
        void Actualizar(Profesor profesor);
        void CambiarEstado(int id, bool activo);
    }
}


