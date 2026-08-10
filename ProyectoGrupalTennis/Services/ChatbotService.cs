using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models.ViewModels;
using System.Globalization;
using System.Text;

namespace ProyectoGrupalTennis.Services
{
    // Módulo 6 - Asistente Virtual
    // USER-06-001: responde preguntas de horarios, precios y ubicación (a partir de las FAQ).
    // USER-06-002: si no encuentra respuesta, ofrece un enlace directo a WhatsApp.
    // USER-06-003: responde consultas sobre canchas libres hoy.
    // ADM-06-003: registra en ConsultaChatbot toda interacción, resuelta o no.
    public class ChatbotService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Palabras muy comunes que no aportan al momento de comparar preguntas
        private static readonly HashSet<string> Stopwords = new()
        {
            "el","la","los","las","de","del","un","una","unos","unas","y","o","a","en","que",
            "es","son","como","cual","cuales","para","por","con","mi","me","tu","su","al",
            "hay","tiene","tienen","quiero","puedo","podria","podría","favor","porfa","hola",
            "buenas","dias","días","tardes","noches","se","si","no","the"
        };

        public ChatbotService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Preguntas rápidas (chips) que se muestran al abrir el chat
        public async Task<List<PreguntaRapidaViewModel>> ObtenerPreguntasRapidasAsync()
        {
            return await _context.PreguntasFrecuentes
                .Where(p => p.Activa)
                .OrderByDescending(p => p.VecesConsultada)
                .ThenBy(p => p.IdPregunta)
                .Take(5)
                .Select(p => new PreguntaRapidaViewModel
                {
                    IdPregunta = p.IdPregunta,
                    Pregunta = p.Pregunta
                })
                .ToListAsync();
        }

        public async Task<ChatRespuestaViewModel> ProcesarMensajeAsync(string? mensaje, string? idUsuario)
        {
            mensaje = (mensaje ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return new ChatRespuestaViewModel
                {
                    Resuelto = true,
                    Respuesta = "Escribe tu consulta y con gusto te ayudo. También puedes tocar una de las preguntas rápidas."
                };
            }

            // USER-06-003: consulta de canchas libres hoy
            if (EsConsultaDeDisponibilidad(mensaje))
            {
                var respuestaCanchas = await ConsultarCanchasLibresHoyAsync(mensaje);

                await RegistrarConsultaAsync(mensaje, respuestaCanchas, resuelto: true, idUsuario);

                return new ChatRespuestaViewModel
                {
                    Resuelto = true,
                    Respuesta = respuestaCanchas
                };
            }

            // USER-06-001: buscar en las preguntas frecuentes (ADM-06-001 las administra)
            var faq = await BuscarFaqAsync(mensaje);

            if (faq != null)
            {
                faq.VecesConsultada++;
                await _context.SaveChangesAsync();

                await RegistrarConsultaAsync(mensaje, faq.Respuesta, resuelto: true, idUsuario);

                return new ChatRespuestaViewModel
                {
                    Resuelto = true,
                    Respuesta = faq.Respuesta
                };
            }

            // USER-06-002 / ADM-06-002 / ADM-06-003: no se encontró respuesta -> se ofrece WhatsApp y se registra el fallo
            await RegistrarConsultaAsync(mensaje, null, resuelto: false, idUsuario);

            var numeroWhatsapp = _configuration["AcademiaSettings:WhatsappSolicitudes"];

            string? whatsappUrl = null;

            if (!string.IsNullOrWhiteSpace(numeroWhatsapp))
            {
                var texto =
                    "Hola, estaba consultando con el asistente virtual y no encontré respuesta a lo siguiente: " +
                    $"\"{mensaje}\". ¿Me pueden ayudar?";

                whatsappUrl = $"https://wa.me/{numeroWhatsapp}?text={Uri.EscapeDataString(texto)}";
            }

            return new ChatRespuestaViewModel
            {
                Resuelto = false,
                Respuesta =
                    "No encontré información sobre eso todavía. Te recomiendo escribirle directamente a la " +
                    "academia por WhatsApp, con gusto te van a ayudar.",
                WhatsappUrl = whatsappUrl
            };
        }

        // ---------------- FAQ matching ----------------

        private async Task<PreguntaFrecuente?> BuscarFaqAsync(string mensaje)
        {
            var tokensMensaje = Tokenizar(mensaje);

            if (tokensMensaje.Count == 0)
            {
                return null;
            }

            var faqs = await _context.PreguntasFrecuentes
                .Where(p => p.Activa)
                .ToListAsync();

            PreguntaFrecuente? mejor = null;
            var mejorPuntaje = 0;

            foreach (var faq in faqs)
            {
                var tokensFaq = Tokenizar(faq.Pregunta + " " + faq.Categoria);
                var puntaje = tokensMensaje.Count(t => tokensFaq.Contains(t));

                if (puntaje > mejorPuntaje)
                {
                    mejorPuntaje = puntaje;
                    mejor = faq;
                }
            }

            // Se exige al menos una coincidencia real de palabra clave para evitar respuestas al azar
            return mejorPuntaje >= 1 ? mejor : null;
        }

        private static List<string> Tokenizar(string texto)
        {
            var normalizado = QuitarAcentos(texto.ToLowerInvariant());

            var palabras = normalizado
                .Split(new[] { ' ', ',', '.', '?', '¿', '!', '¡', ':', ';', '\n', '\r', '\t' },
                    StringSplitOptions.RemoveEmptyEntries);

            return palabras
                .Where(p => p.Length >= 3 && !Stopwords.Contains(p))
                .Distinct()
                .ToList();
        }

        private static string QuitarAcentos(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        // ---------------- USER-06-003: disponibilidad de canchas ----------------

        private static bool EsConsultaDeDisponibilidad(string mensaje)
        {
            var texto = QuitarAcentos(mensaje.ToLowerInvariant());

            var mencionaCancha = texto.Contains("cancha");
            var mencionaDisponibilidad =
                texto.Contains("libre") ||
                texto.Contains("disponib") ||
                texto.Contains("hoy hay");

            return mencionaCancha && mencionaDisponibilidad;
        }

        private static readonly CultureInfo CulturaEspanol = new("es-ES");

        private async Task<string> ConsultarCanchasLibresHoyAsync(string mensaje)
        {
            var texto = QuitarAcentos(mensaje.ToLowerInvariant());
            var hoy = DateTime.Today;

            // Filtro por tipo de cancha si el alumno lo menciona (criterio de aceptación 2)
            string? tipoSolicitado = null;

            if (texto.Contains("padel")) tipoSolicitado = "padel";
            else if (texto.Contains("pickleball")) tipoSolicitado = "pickleball";
            else if (texto.Contains("tenis") || texto.Contains("tennis")) tipoSolicitado = "tenis";

            var disponiblesHoy = await BuscarDisponiblesAsync(hoy, tipoSolicitado);

            if (disponiblesHoy.Any())
            {
                var lineas = disponiblesHoy.Select(r =>
                    $"• {r.Cancha!.Nombre}: {r.HoraInicio:hh\\:mm} - {r.HoraFin:hh\\:mm}");

                return "Estas son las canchas libres para hoy:\n" + string.Join("\n", lineas) +
                       "\n\nPuedes ir a la sección \"Canchas disponibles\" para reservar el horario que prefieras.";
            }

            // Si no hay nada para hoy, se busca el próximo día con disponibilidad (mejora sobre el criterio base)
            for (var offset = 1; offset <= 7; offset++)
            {
                var fecha = hoy.AddDays(offset);
                var disponibles = await BuscarDisponiblesAsync(fecha, tipoSolicitado);

                if (disponibles.Any())
                {
                    var tipoTextoProximo = tipoSolicitado != null ? $" de {tipoSolicitado}" : "";
                    var lineas = disponibles.Select(r =>
                        $"• {r.Cancha!.Nombre}: {r.HoraInicio:hh\\:mm} - {r.HoraFin:hh\\:mm}");

                    return $"Por ahora no hay espacios libres{tipoTextoProximo} para hoy, pero sí encontré para el " +
                           $"{fecha.ToString("dddd d 'de' MMMM", CulturaEspanol)}:\n" + string.Join("\n", lineas) +
                           "\n\nPuedes ir a la sección \"Canchas disponibles\" para reservar el horario que prefieras.";
                }
            }

            var tipoTexto = tipoSolicitado != null ? $" de {tipoSolicitado}" : "";
            return $"Por ahora no veo espacios libres{tipoTexto} para los próximos días. Puedes revisar todo el " +
                   "calendario en la sección \"Canchas disponibles\" o consultar directamente con la academia.";
        }

        private async Task<List<Reserva>> BuscarDisponiblesAsync(DateTime fecha, string? tipoSolicitado)
        {
            var query = _context.Reservas
                .Include(r => r.Cancha)
                .Where(r =>
                    r.IdAlumno == null &&
                    r.Estado == "Disponible" &&
                    r.FechaReserva.Date == fecha.Date)
                .AsQueryable();

            if (tipoSolicitado != null)
            {
                query = query.Where(r => r.Cancha != null &&
                    EF.Functions.Like(r.Cancha.Nombre, $"%{tipoSolicitado}%"));
            }

            return await query
                .OrderBy(r => r.HoraInicio)
                .Take(8)
                .ToListAsync();
        }

        // ---------------- ADM-06-003: log de consultas ----------------

        private async Task RegistrarConsultaAsync(string mensaje, string? respuesta, bool resuelto, string? idUsuario)
        {
            _context.ConsultasChatbot.Add(new ConsultaChatbot
            {
                IdUsuario = string.IsNullOrWhiteSpace(idUsuario) ? null : idUsuario,
                MensajeUsuario = mensaje.Length > 500 ? mensaje[..500] : mensaje,
                RespuestaBot = respuesta,
                FechaConsulta = DateTime.Now,
                Resuelto = resuelto
            });

            await _context.SaveChangesAsync();
        }
    }
}