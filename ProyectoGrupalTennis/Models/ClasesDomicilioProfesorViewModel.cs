namespace ProyectoGrupalTennis.Models
{
    public class ClasesDomicilioProfesorViewModel
    {
        public string? FiltroCurso { get; set; }

        public List<string> CursosDisponibles { get; set; } = new();

        public List<ClaseDomicilioProfesorItemViewModel> Clases { get; set; }
            = new();
    }

    public class ClaseDomicilioProfesorItemViewModel
    {
        public int IdMatricula { get; set; }

        public int IdCurso { get; set; }

        public string Curso { get; set; } = string.Empty;

        public string Nivel { get; set; } = string.Empty;

        public string Alumno { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Zona { get; set; } = string.Empty;

        public decimal Latitud { get; set; }

        public decimal Longitud { get; set; }

        public decimal DistanciaKm { get; set; }

        public List<string> Horarios { get; set; } = new();
    }
}