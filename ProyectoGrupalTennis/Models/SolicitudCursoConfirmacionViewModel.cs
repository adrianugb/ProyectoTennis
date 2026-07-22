namespace ProyectoGrupalTennis.Models
{
    public class SolicitudCursoConfirmacionViewModel
    {
        public string NombreCurso { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public string Disponibilidad { get; set; } = string.Empty;
        public string? Comentarios { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int IdSolicitudCurso { get; set; }
        public string? WhatsappUrl { get; set; }
    }

}