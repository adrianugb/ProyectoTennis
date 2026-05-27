using AcademiaTennisDAL.Entities;
using AcademiaTennisDAL.Repositories;

namespace AcademiaTennisBLL.Services
{
    public class ProfesorService : IProfesorService
    {
        private readonly IProfesorRepository _repo;

        public ProfesorService(IProfesorRepository repo)
        {
            _repo = repo;
        }

        public List<Profesor> ObtenerTodos() => _repo.ObtenerTodos();

        public Profesor? ObtenerPorId(int id) => _repo.ObtenerPorId(id);

        public void Agregar(Profesor profesor)
        {
            if (string.IsNullOrWhiteSpace(profesor.Nombre))
                throw new Exception("El nombre es obligatorio.");
            profesor.FechaCreacion = DateTime.UtcNow;
            _repo.Agregar(profesor);
        }

        public void Actualizar(Profesor profesor)
        {
            if (string.IsNullOrWhiteSpace(profesor.Nombre))
                throw new Exception("El nombre es obligatorio.");
            _repo.Actualizar(profesor);
        }

        public void CambiarEstado(int id, bool activo) =>
            _repo.CambiarEstado(id, activo);
    }
}