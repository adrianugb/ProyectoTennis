using AcademiaTennisDAL.Entities;

namespace AcademiaTennisBLL.Services
{
    public interface ICursoService
    {
        List<Curso> ObtenerTodos();
        Curso? ObtenerPorId(int id);
        void Agregar(Curso clase, List<Horario> horarios);
        void Actualizar(Curso clase, List<Horario> horarios);
        void CambiarEstado(int id, bool activo);
        List<Profesor> ObtenerProfesores();
    }
}