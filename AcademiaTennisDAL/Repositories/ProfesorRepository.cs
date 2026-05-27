using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;

namespace AcademiaTennisDAL.Repositories
{
    public class ProfesorRepository : IProfesorRepository
    {
        private readonly AppDbContext _context;

        public ProfesorRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Profesor> ObtenerTodos() =>
            _context.Profesores.OrderBy(p => p.Nombre).ToList();

        public Profesor? ObtenerPorId(int id) =>
            _context.Profesores.Find(id);

        public void Agregar(Profesor profesor)
        {
            _context.Profesores.Add(profesor);
            _context.SaveChanges();
        }

        public void Actualizar(Profesor profesor)
        {
            _context.Profesores.Update(profesor);
            _context.SaveChanges();
        }

        public void CambiarEstado(int id, bool activo)
        {
            var profesor = _context.Profesores.Find(id);
            if (profesor != null)
            {
                profesor.Activo = activo;
                _context.SaveChanges();
            }
        }
    }
}