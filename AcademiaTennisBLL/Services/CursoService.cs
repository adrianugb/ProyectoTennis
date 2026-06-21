using AcademiaTennisDAL.Entities;
using AcademiaTennisDAL.Repositories;

namespace AcademiaTennisBLL.Services
{
    public class CursoService : ICursoService
    {
        private readonly ICursoRepository _repo;

        public CursoService(ICursoRepository repo)
        {
            _repo = repo;
        }

        public List<Curso> ObtenerTodos() => _repo.ObtenerTodos();

        public Curso? ObtenerPorId(int id) => _repo.ObtenerPorId(id);

        public void Agregar(Curso Curso, List<Horario> horarios)
        {
            if (string.IsNullOrWhiteSpace(Curso.Nombre))
                throw new Exception("El nombre es obligatorio.");
            if (Curso.CuposDisponibles < 0)
                throw new Exception("Los cupos no pueden ser negativos.");
            if (horarios == null || horarios.Count == 0)
                throw new Exception("Debe definir al menos un horario.");

            _repo.Agregar(Curso, horarios);
        }

        public void Actualizar(Curso Curso, List<Horario> horarios)
        {
            if (string.IsNullOrWhiteSpace(Curso.Nombre))
                throw new Exception("El nombre es obligatorio.");
            if (horarios == null || horarios.Count == 0)
                throw new Exception("Debe definir al menos un horario.");

            _repo.Actualizar(Curso, horarios);
        }

        public void CambiarEstado(int id, bool activo) =>
            _repo.CambiarEstado(id, activo);

        public List<Profesor> ObtenerProfesores() => _repo.ObtenerProfesores();
    }
}