namespace ProyectoGrupalTennis.Models
{
    public class MapaLogisticoAdminViewModel
    {
        public int? FiltroProfesor { get; set; }

        public int? FiltroZona { get; set; }

        public DateTime? FiltroFecha { get; set; }

        public List<OpcionProfesorMapaViewModel> Profesores { get; set; }
            = new();

        public List<OpcionZonaMapaViewModel> Zonas { get; set; }
            = new();

        public List<ClaseMapaAdminItemViewModel> Clases { get; set; }
            = new();
    }

    public class OpcionProfesorMapaViewModel
    {
        public int IdProfesor { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public class OpcionZonaMapaViewModel
    {
        public int IdZona { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public class ClaseMapaAdminItemViewModel
    {
        public int IdMatricula { get; set; }

        public int IdCurso { get; set; }

        public int IdProfesor { get; set; }

        public int? IdZona { get; set; }

        public string Alumno { get; set; } = string.Empty;

        public string Profesor { get; set; } = string.Empty;

        public string Curso { get; set; } = string.Empty;

        public string Nivel { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Zona { get; set; } = string.Empty;

        public decimal Latitud { get; set; }

        public decimal Longitud { get; set; }

        public DateTime Fecha { get; set; }

        public string DiaSemana { get; set; } = string.Empty;

        public string HoraInicio { get; set; } = string.Empty;

        public string HoraFin { get; set; } = string.Empty;

        public string TelefonoAlumno { get; set; } = string.Empty;
    }
}