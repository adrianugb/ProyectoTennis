using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ProyectoGrupalTennis.Services
{
    public class GoogleCalendarService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public GoogleCalendarService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        // URL para redirigir al usuario a autorizar Google Calendar
        public string ObtenerUrlAutorizacion(string userId)
        {
            var flow = CrearFlow();
            var uri = flow.CreateAuthorizationCodeRequest(
                _config["GoogleCalendar:RedirectUri"]!);
            uri.State = userId; // guardamos el userId para recuperarlo en el callback
            uri.Scope = "https://www.googleapis.com/auth/calendar.events";
            return uri.Build().ToString();
        }

        // Intercambia el código por un token y lo guarda en BD
        public async Task GuardarTokenAsync(string userId, string code)
        {
            var flow = CrearFlow();
            var token = await flow.ExchangeCodeForTokenAsync(
                userId, code,
                _config["GoogleCalendar:RedirectUri"]!,
                CancellationToken.None);

            var existente = await _context.GoogleCalendarTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (existente != null)
            {
                existente.AccessToken = token.AccessToken;
                existente.RefreshToken = token.RefreshToken ?? existente.RefreshToken;
                existente.Expiry = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600);
            }
            else
            {
                _context.GoogleCalendarTokens.Add(new GoogleCalendarToken
                {
                    UserId = userId,
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken ?? string.Empty,
                    Expiry = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600)
                });
            }

            await _context.SaveChangesAsync();
        }

        // Verifica si el usuario ya autorizó Google Calendar
        public async Task<bool> TieneTokenAsync(string userId)
        {
            return await _context.GoogleCalendarTokens
                .AnyAsync(t => t.UserId == userId && t.RefreshToken != string.Empty);
        }

        // Crea un evento en el Google Calendar del usuario
        public async Task<string?> CrearEventoAsync(
            string userId,
            string titulo,
            string descripcion,
            DateTime fecha,
            TimeSpan horaInicio,
            TimeSpan horaFin)
        {
            var service = await ObtenerServicioAsync(userId);
            if (service == null) return null;

            var inicio = fecha.Date + horaInicio;
            var fin = fecha.Date + horaFin;

            var evento = new Event
            {
                Summary = titulo,
                Description = descripcion,
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(inicio, TimeZoneInfo.Local.GetUtcOffset(inicio))
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(fin, TimeZoneInfo.Local.GetUtcOffset(fin))
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new EventReminder { Method = "popup", Minutes = 60 },
                        new EventReminder { Method = "email", Minutes = 1440 } // 24h antes
                    }
                }
            };

            var request = service.Events.Insert(evento, "primary");
            var resultado = await request.ExecuteAsync();
            return resultado.Id; // ID del evento en Google Calendar
        }

        // Actualiza un evento existente
        public async Task ActualizarEventoAsync(
            string userId,
            string googleEventId,
            string titulo,
            string descripcion,
            DateTime fecha,
            TimeSpan horaInicio,
            TimeSpan horaFin)
        {
            var service = await ObtenerServicioAsync(userId);
            if (service == null) return;

            var inicio = fecha.Date + horaInicio;
            var fin = fecha.Date + horaFin;

            var evento = new Event
            {
                Summary = titulo,
                Description = descripcion,
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(inicio, TimeZoneInfo.Local.GetUtcOffset(inicio))
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(fin, TimeZoneInfo.Local.GetUtcOffset(fin))
                }
            };

            await service.Events.Update(evento, "primary", googleEventId).ExecuteAsync();
        }

        // Elimina un evento
        public async Task EliminarEventoAsync(string userId, string googleEventId)
        {
            var service = await ObtenerServicioAsync(userId);
            if (service == null) return;

            await service.Events.Delete("primary", googleEventId).ExecuteAsync();
        }

        // ── Privados ──────────────────────────────────────────

        private GoogleAuthorizationCodeFlow CrearFlow() =>
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _config["GoogleCalendar:ClientId"]!,
                    ClientSecret = _config["GoogleCalendar:ClientSecret"]!
                },
                Scopes = new[] { "https://www.googleapis.com/auth/calendar.events" }
            });

        private async Task<CalendarService?> ObtenerServicioAsync(string userId)
        {
            var tokenData = await _context.GoogleCalendarTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (tokenData == null)
            {
                return null;
            }

            var segundosRestantes =
                (long)(tokenData.Expiry - DateTime.UtcNow).TotalSeconds;

            var token = new TokenResponse
            {
                AccessToken = tokenData.AccessToken,
                RefreshToken = tokenData.RefreshToken,

                // Evita valores negativos que puedan provocar
                // errores de DateTime dentro de la librería de Google.
                ExpiresInSeconds =
                    segundosRestantes > 0
                        ? segundosRestantes
                        : 0,

                IssuedUtc = DateTime.UtcNow
            };

            var flow = CrearFlow();

            var credential =
                new UserCredential(
                    flow,
                    userId,
                    token);

            return new CalendarService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Academia de Tennis"
                });
        }



        // ── Métodos adicionales para el controlador ──────────────────────────────────

        // Estos van dentro de la clase GoogleCalendarService (agregálos al archivo principal)
        public async Task<GoogleCalendarToken?> ObtenerTokenAsync(string userId) =>
        await _context.GoogleCalendarTokens.FirstOrDefaultAsync(t => t.UserId == userId);

        public async Task EliminarTokenAsync(string userId)
        {
            var token = await _context.GoogleCalendarTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            if (token != null)
            {
                _context.GoogleCalendarTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }
    }
}
