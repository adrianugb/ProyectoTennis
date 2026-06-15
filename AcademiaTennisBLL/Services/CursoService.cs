using AcademiaTennisDAL.Entities;
using AcademiaTennisDAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

   
        public List<Profesor> ObtenerProfesores() => _repo.ObtenerProfesores();

        public void Agregar(Curso curso)
        {
            if (string.IsNullOrWhiteSpace(curso.Nombre))
                throw new Exception("El nombre es obligatorio.");
            if (curso.CuposDisponibles < 0)
                throw new Exception("Los cupos no pueden ser negativos.");
            _repo.Agregar(curso);
        }

        public void Actualizar(Curso curso)
        {
            if (string.IsNullOrWhiteSpace(curso.Nombre))
                throw new Exception("El nombre es obligatorio.");
            _repo.Actualizar(curso);
        }

        public void CambiarEstado(int id, bool activo) =>
            _repo.CambiarEstado(id, activo);


        public List<Horario> ObtenerHorarios(int idCurso) =>
    _repo.ObtenerHorarios(idCurso);

        public void AgregarHorario(Horario horario)
        {
            if (string.IsNullOrWhiteSpace(horario.DiaSemana))
                throw new Exception("El día es obligatorio.");
            if (horario.HoraFin <= horario.HoraInicio)
                throw new Exception("La hora de fin debe ser mayor a la hora de inicio.");
            _repo.AgregarHorario(horario);
        }

        public void EliminarHorario(int idHorario) =>
            _repo.EliminarHorario(idHorario);


    }
}
