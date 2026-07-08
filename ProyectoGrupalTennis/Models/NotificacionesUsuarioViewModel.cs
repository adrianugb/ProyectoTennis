namespace ProyectoGrupalTennis.Models
{
    public class NotificacionesUsuarioViewModel
    {
        public List<NotificacionUsuarioItemViewModel> Notificaciones { get; set; } = new();

        // USER-09-008: canal preferido (Email / SMS / Push / WhatsApp)
        public string CanalPreferido { get; set; } = "Email";

        // USER-09-009: tipos de notificacion que el alumno quiere recibir
        public bool NotificacionesPago { get; set; } = true;
        public bool NotificacionesClase { get; set; } = true;
        public bool NotificacionesRecordatorio { get; set; } = true;
        public bool NotificacionesCampeonato { get; set; } = true;
    }

    public class NotificacionUsuarioItemViewModel
    {
        public int IdNotificacion { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public bool Leida { get; set; }

        public DateTime FechaEnvio { get; set; }

        // USER-09-008: canal por el que quedó disponible la notificación y si el
        // canal preferido (correo) falló y se tuvo que usar la plataforma como respaldo.
        public string CanalUsado { get; set; } = "Plataforma";

        public bool EnvioFallido { get; set; }
    }
}