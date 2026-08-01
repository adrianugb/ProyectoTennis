using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using AcademiaTennisDAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcademiaTennisBLL.Services
{
    public class CursoService : ICursoService
    {
        private readonly ICursoRepository _repo;
        private readonly AppDbContext _context;

        public CursoService(
            ICursoRepository repo,
            AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public List<Curso> ObtenerTodos() => _repo.ObtenerTodos();

        public Curso? ObtenerPorId(int id) => _repo.ObtenerPorId(id);

        public void Agregar(Curso Curso, List<Horario> horarios)
        {
            if (string.IsNullOrWhiteSpace(Curso.Nombre))
                throw new Exception("El nombre es obligatorio.");
            if (Curso.CuposDisponibles < 0)
                throw new Exception("Los cupos no pueden ser negativos.");
            if (Curso.Precio <= 0)
                throw new Exception("El precio del curso debe ser mayor a cero.");
            if (horarios == null || horarios.Count == 0)
                throw new Exception("Debe definir al menos un horario.");

            _repo.Agregar(Curso, horarios);
        }

        public void Actualizar(Curso Curso, List<Horario> horarios)
        {
            if (string.IsNullOrWhiteSpace(Curso.Nombre))
                throw new Exception("El nombre es obligatorio.");

            if (Curso.Precio <= 0)
                throw new Exception("El precio del curso debe ser mayor a cero.");

            if (horarios == null || horarios.Count == 0)
                throw new Exception("Debe definir al menos un horario.");

            _repo.Actualizar(Curso, horarios);
        }

        public void CambiarEstado(int id, bool activo) =>
            _repo.CambiarEstado(id, activo);

        public List<Profesor> ObtenerProfesores() => _repo.ObtenerProfesores();

        public List<Horario> ObtenerHorarios(int idCurso) => _repo.ObtenerHorarios(idCurso);
        public void AgregarHorario(Horario horario) => _repo.AgregarHorario(horario);
        public void EliminarHorario(int idHorario) => _repo.EliminarHorario(idHorario);

        public async Task ActualizarCursosFinalizadosAsync()
        {
            DateTime ahora = DateTime.Now;

            var cursosActivos = await _context.Cursos
                .Include(c => c.Horarios)
                .Where(c => c.Activo)
                .ToListAsync();

            bool huboCambios = false;

            foreach (var curso in cursosActivos)
            {
                if (curso.Horarios == null ||
                    !curso.Horarios.Any())
                {
                    continue;
                }

                DateTime ultimoFinal = curso.Horarios
                    .Select(h =>
                        h.Fecha.Date.Add(h.HoraFin))
                    .Max();

                if (ultimoFinal <= ahora)
                {
                    curso.Activo = false;
                    huboCambios = true;
                }
            }

            if (huboCambios)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}