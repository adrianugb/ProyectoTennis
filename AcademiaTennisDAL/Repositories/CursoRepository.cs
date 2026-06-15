using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.EntityFrameworkCore;

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

        public List<Profesor> ObtenerProfesores() =>
             _context.Profesores.OrderBy(p => p.Nombre).ToList();

        public Profesor? ObtenerProfesorporId(int id) =>
            _context.Profesores.Find(id);

        public Curso? ObtenerPorId(int id) =>
    _context.Cursos
        .Include(c => c.Profesor)
        .Include(c => c.Horarios)
        .FirstOrDefault(c => c.IdCurso == id);

        public List<Horario> ObtenerHorarios(int idCurso) =>
            _context.Horarios
                .Where(h => h.IdCurso == idCurso)
                .OrderBy(h => h.DiaSemana)
                .ToList();

        public void AgregarHorario(Horario horario)
        {
            _context.Horarios.Add(horario);
            _context.SaveChanges();
        }

        public void EliminarHorario(int idHorario)
        {
            var horario = _context.Horarios.Find(idHorario);
            if (horario != null)
            {
                _context.Horarios.Remove(horario);
                _context.SaveChanges();
            }
        }

    }
}