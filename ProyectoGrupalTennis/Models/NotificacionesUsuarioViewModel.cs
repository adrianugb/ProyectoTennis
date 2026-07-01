namespace ProyectoGrupalTennis.Models
{
    public class NotificacionesUsuarioViewModel
    {
        public List<NotificacionUsuarioItemViewModel> Notificaciones { get; set; } = new();
    }

    public class NotificacionUsuarioItemViewModel
    {
        public int IdNotificacion { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public bool Leida { get; set; }

        public DateTime FechaEnvio { get; set; }
    }
}