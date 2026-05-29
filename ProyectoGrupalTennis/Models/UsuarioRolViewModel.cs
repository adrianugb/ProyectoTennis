using AcademiaTennisDAL.Entities;

namespace ProyectoGrupalTennis.Models.ViewModels
{
    public class UsuarioRolViewModel
    {
        public ApplicationUser Usuario { get; set; }

        public string Rol { get; set; }
    }
}