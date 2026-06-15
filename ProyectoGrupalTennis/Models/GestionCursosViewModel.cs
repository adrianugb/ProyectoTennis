namespace ProyectoGrupalTennis.Models
{
    public class GestionCursosViewModel
    {
        public List<CursoAdminItemViewModel> Cursos { get; set; } = new();
        public List<ProfesorSelectViewModel> Profesores { get; set; } = new();
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }
        public string? FiltroBuscar { get; set; }
    }
}
