using AcademiaTennisDAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcademiaTennisDAL.Repositories
{
   public interface IProfesorRepository
    {
        
        List<Profesor> ObtenerTodos();
        Profesor? ObtenerPorId(int id);
        void Agregar(Profesor profesor);
        void Actualizar(Profesor profesor);
        void CambiarEstado(int id, bool activo);
    }
}


