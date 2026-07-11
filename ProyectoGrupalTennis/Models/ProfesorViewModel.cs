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

    // ── MisCursos con horarios editables (IdHorario + Fecha) ──

    public class HorarioProfesorItemViewModel
    {
        public int IdHorario { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;      // yyyy-MM-dd
        public string HoraInicio { get; set; } = string.Empty; // HH:mm
        public string HoraFin { get; set; } = string.Empty;    // HH:mm
        public string Texto => $"{DiaSemana} {HoraInicio} - {HoraFin}";
    }

    public class CursoProfesorItemViewModel
    {
        public int IdCurso { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
        public bool Activo { get; set; }
        public List<HorarioProfesorItemViewModel> Horarios { get; set; } = new();
    }

    public class ProfesorMisCursosViewModel
    {
        public List<CursoProfesorItemViewModel> Cursos { get; set; } = new();
        public string? FiltroBuscar { get; set; }
        public string? FiltroNivel { get; set; }
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }
    }

    // ── Historia 6: Profesor visualiza alumnos ──

    public class AlumnoProfesorListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FechaClase { get; set; } = string.Empty;
        public string HoraClase { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Clase { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string EstadoMatricula { get; set; } = string.Empty;
        public string DiaSemana { get; set; } = string.Empty;
        public int HoraInicioEntera { get; set; }
        public List<string> CursosMatriculados { get; set; } = new(); // ← corregido
    }

    public class ProfesorAlumnosViewModel
    {
        public IList<AlumnoProfesorListItemViewModel> Alumnos { get; set; } = new List<AlumnoProfesorListItemViewModel>();
        public string? FiltroBuscar { get; set; }
        public string? FiltroCurso { get; set; }
        public string? FiltroFecha { get; set; }
        public List<string> CursosDisponibles { get; set; } = new();
    }

    // ── Reprogramar clase (profesor) ──

    public class ReprogramarClaseViewModel
    {
        public int IdHorario { get; set; }
        public int IdCurso { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;      // yyyy-MM-dd
        public string HoraInicio { get; set; } = string.Empty; // HH:mm
        public string HoraFin { get; set; } = string.Empty;    // HH:mm
    }
}