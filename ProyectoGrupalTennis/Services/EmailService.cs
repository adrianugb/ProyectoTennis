using System.Net;
using System.Net.Mail;
using ProyectoGrupalTennis.Models;

namespace ProyectoGrupalTennis.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task EnviarCorreoAsync(
            string destinatario,
            string asunto,
            string mensaje)
        {
            using var smtp = new SmtpClient(_settings.SmtpServer)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(
                    _settings.SenderEmail,
                    _settings.SenderPassword),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(
                    _settings.SenderEmail,
                    _settings.SenderName),
                Subject = asunto,
                Body = mensaje,
                IsBodyHtml = true
            };

            mail.To.Add(destinatario);

            await smtp.SendMailAsync(mail);
        }
    }
}