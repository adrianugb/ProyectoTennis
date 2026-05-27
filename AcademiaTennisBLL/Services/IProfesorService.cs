using AcademiaTennisDAL.Entities;

namespace AcademiaTennisBLL.Services
{
    public interface IProfesorService
    {
        List<Profesor> ObtenerTodos();
        Profesor? ObtenerPorId(int id);
        void Agregar(Profesor profesor);
        void Actualizar(Profesor profesor);
        void CambiarEstado(int id, bool activo);
    }
}