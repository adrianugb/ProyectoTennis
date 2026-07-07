namespace ProyectoGrupalTennis.Models
{
    // ── USER-04-002: Alumno visualiza cursos ──

    public class CursoUsuarioItemViewModel
    {
        public int IdCurso { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
        public decimal Precio { get; set; }
        public string? NombreProfesor { get; set; }
        public List<string> Horarios { get; set; } = new();
    }

    public class UsuarioCursosViewModel
    {
        public IList<CursoUsuarioItemViewModel> Cursos { get; set; } = new List<CursoUsuarioItemViewModel>();
        public string? FiltroBuscar { get; set; }
        public string? FiltroNivel { get; set; }
    }

    // ── USER-04-003: Alumno visualiza horarios ──

    public class HorarioUsuarioItemViewModel
    {
        public int IdHorario { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
    }

    public class UsuarioHorariosViewModel
    {
        public IList<HorarioUsuarioItemViewModel> Horarios { get; set; } = new List<HorarioUsuarioItemViewModel>();
        public string? FiltroDia { get; set; }
        public string? FiltroBuscar { get; set; }
        public List<string> DiasDisponibles { get; set; } = new();
    }

    // ── ADM-04-006: Admin asigna profesor a curso ──

    public class CursoAdminItemViewModel
    {
        public int IdCurso { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public string? NombreProfesor { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public List<string> Horarios { get; set; } = new();
    }

    public class ProfesorSelectViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
    }

    
}