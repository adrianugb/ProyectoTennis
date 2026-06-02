using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;

namespace AcademiaTennisDAL.Repositories
{
 
        public class CursoRepository : ICursoRepository
        {
            private readonly AppDbContext _context;

            public CursoRepository(AppDbContext context)
            {
                _context = context;
            }

            public List<Curso> ObtenerTodos() =>
                _context.Cursos.OrderBy(c => c.Nombre).ToList();

            public Curso? ObtenerPorId(int id) =>
                _context.Cursos.Find(id);

            public void Agregar(Curso curso)
            {
                _context.Cursos.Add(curso);
                _context.SaveChanges();
            }

            public void Actualizar(Curso curso)
            {
                _context.Cursos.Update(curso);
                _context.SaveChanges();
            }

            public void CambiarEstado(int id, bool activo)
            {
                var curso = _context.Cursos.Find(id);
                if (curso != null)
                {
                    curso.Activo = activo;
                    _context.SaveChanges();
                }
            }
        }
    }