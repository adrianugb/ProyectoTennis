using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcademiaTennisDAL.Entities;

namespace AcademiaTennisBLL.Services
{
    public interface ICursoService
    {
        List<Curso> ObtenerTodos();
        List<Profesor> ObtenerProfesores();
        Curso? ObtenerPorId(int id);
        void Agregar(Curso curso);
        void Actualizar(Curso curso);
        void CambiarEstado(int id, bool activo);

        List<Horario> ObtenerHorarios(int idCurso);
        void AgregarHorario(Horario horario);
        void EliminarHorario(int idHorario);
    }
}
