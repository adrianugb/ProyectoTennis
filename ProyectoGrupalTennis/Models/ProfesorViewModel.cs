namespace ProyectoGrupalTennis.Models
{
    // ── Historia 5: Profesor visualiza cursos ──

    public class CursoListItemViewModel
    {
        public int IdCurso { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
        public bool Activo { get; set; }
        public List<string> Horarios { get; set; } = new();
    }

    public class ProfesorCursosViewModel
    {
        public IList<CursoListItemViewModel> Cursos { get; set; } = new List<CursoListItemViewModel>();
        public string? FiltroBuscar { get; set; }
        public string? FiltroNivel { get; set; }
    }

    // ── Historia 6: Profesor visualiza alumnos ──

    public class AlumnoProfesorListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public List<string> CursosMatriculados { get; set; } = new();
    }

    public class ProfesorAlumnosViewModel
    {
        public IList<AlumnoProfesorListItemViewModel> Alumnos { get; set; } = new List<AlumnoProfesorListItemViewModel>();
        public string? FiltroBuscar { get; set; }
        public string? FiltroCurso { get; set; }
        public List<string> CursosDisponibles { get; set; } = new();
    }
}