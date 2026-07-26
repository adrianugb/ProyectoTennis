using System.ComponentModel.DataAnnotations;

namespace ProyectoGrupalTennis.Models.ViewModels
{
    // ---------- Chat (lado usuario) ----------

    // USER-06-001 / USER-06-002 / USER-06-003: mensaje que envía el usuario al bot
    public class ChatMensajeRequest
    {
        [Required]
        [StringLength(500)]
        public string Mensaje { get; set; } = string.Empty;
    }

    // Respuesta que arma el bot para mostrar en el widget
    public class ChatRespuestaViewModel
    {
        public string Respuesta { get; set; } = string.Empty;

        // USER-06-001/003: true cuando el bot sí encontró información
        public bool Resuelto { get; set; }

        // USER-06-002: si no se resolvió, se arma un link directo a WhatsApp
        public string? WhatsappUrl { get; set; }
    }

    // Botones de preguntas rápidas que se muestran al abrir el chat
    public class PreguntaRapidaViewModel
    {
        public int IdPregunta { get; set; }
        public string Pregunta { get; set; } = string.Empty;
    }

    // ---------- Administración de FAQ (ADM-06-001) ----------

    public class AdminFaqIndexViewModel
    {
        public string? FiltroBuscar { get; set; }
        public string? FiltroCategoria { get; set; }
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }
        public List<AdminFaqItemViewModel> Preguntas { get; set; } = new();
    }

    public class AdminFaqItemViewModel
    {
        public int IdPregunta { get; set; }

        [Required(ErrorMessage = "La pregunta es obligatoria.")]
        [StringLength(300)]
        public string Pregunta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La respuesta es obligatoria.")]
        [StringLength(2000)]
        public string Respuesta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una categoría.")]
        [StringLength(50)]
        public string Categoria { get; set; } = "General";

        public bool Activa { get; set; } = true;

        public int VecesConsultada { get; set; }
    }

    // ---------- Consultas fallidas (ADM-06-003) ----------

    public class AdminConsultaFallidaViewModel
    {
        public int IdConsulta { get; set; }
        public string MensajeUsuario { get; set; } = string.Empty;
        public DateTime FechaConsulta { get; set; }
        public string Usuario { get; set; } = "Invitado";
    }

    public class AdminConsultasFallidasIndexViewModel
    {
        public List<AdminConsultaFallidaViewModel> Consultas { get; set; } = new();

        // Ranking de frases más repetidas que el bot no pudo resolver
        public List<FraseFrecuenteViewModel> RankingFrasesSinRespuesta { get; set; } = new();

        public int TotalConsultas { get; set; }
        public int TotalNoResueltas { get; set; }
        public int TotalRedirigidasWhatsapp { get; set; }
    }

    public class FraseFrecuenteViewModel
    {
        public string Frase { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
