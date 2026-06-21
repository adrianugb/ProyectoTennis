// CursoRepository.cs
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
            _context.Cursos
                .Include(c => c.Profesor)
                .Include(c => c.Horarios)
                .OrderBy(c => c.Nombre)
                .ToList();

        public Curso? ObtenerPorId(int id) =>
            _context.Cursos
                .Include(c => c.Profesor)
                .Include(c => c.Horarios)
                .FirstOrDefault(c => c.IdCurso == id);

        public void Agregar(Curso Curso, List<Horario> horarios)
        {
            _context.Cursos.Add(Curso);
            _context.SaveChanges(); // genera IdCurso

            foreach (var h in horarios)
            {
                h.IdCurso = Curso.IdCurso;
                _context.Horarios.Add(h);
            }
            _context.SaveChanges();
        }

        public void Actualizar(Curso Curso, List<Horario> horarios)
        {
            _context.Cursos.Update(Curso);

            // Reemplazar horarios existentes
            var horariosActuales = _context.Horarios
                .Where(h => h.IdCurso == Curso.IdCurso)
                .ToList();
            _context.Horarios.RemoveRange(horariosActuales);

            foreach (var h in horarios)
            {
                h.IdCurso = Curso.IdCurso;
                h.IdHorario = 0; // forzar insert nuevo
                _context.Horarios.Add(h);
            }

            _context.SaveChanges();
        }

        public void CambiarEstado(int id, bool activo)
        {
            var Curso = _context.Cursos.Find(id);
            if (Curso != null)
            {
                Curso.Activo = activo;
                _context.SaveChanges();
            }
        }

        public List<Profesor> ObtenerProfesores() =>
            _context.Profesores
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToList();
    }
}