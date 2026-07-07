namespace ProyectoGrupalTennis.Models
{
    public class NotificacionesProfesorViewModel
    {
        public List<NotificacionProfesorItemViewModel> Notificaciones { get; set; } = new();
    }

    public class NotificacionProfesorItemViewModel
    {
        public int IdNotificacion { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public DateTime FechaEnvio { get; set; }

        public string Icono => Tipo switch
        {
            "PROF-09-001" => "fa-check-circle",
            "PROF-09-002" => "fa-clock-o",
            "PROF-09-003" => "fa-times-circle",
            "PROF-09-004" => "fa-bell",
            _ => "fa-bell"
        };

        public string Color => Tipo switch
        {
            "PROF-09-001" => "#a5c422",
            "PROF-09-002" => "#f0a500",
            "PROF-09-003" => "#e05656",
            "PROF-09-004" => "#a5c422",
            _ => "#a5c422"
        };
    }
}
