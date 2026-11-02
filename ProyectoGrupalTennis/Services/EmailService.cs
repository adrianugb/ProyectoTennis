using AcademiaTennisDAL.Context;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ProyectoGrupalTennis.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly AppDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            EmailSettings settings,
            AppDbContext context,
            ILogger<EmailService> logger)
        {
            _settings = settings;
            _context = context;
            _logger = logger;
        }

        public async Task EnviarCorreoAsync(
            string destinatario,
            string asunto,
            string mensaje)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando proceso de envío de correo.");

                // Consultar configuración guardada en MySQL
                var configuracion = await _context.ConfiguracionesAcademia
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == 1);

                // Valores predeterminados desde la configuración del servidor
                var smtpServer = _settings.SmtpServer;
                var smtpPort = _settings.Port;
                var senderEmail = _settings.SenderEmail;
                var senderName = _settings.SenderName;
                var senderPassword = _settings.SenderPassword;

                // Utilizar configuración de MySQL si existe
                if (configuracion != null)
                {
                    if (!string.IsNullOrWhiteSpace(configuracion.SmtpServer))
                    {
                        smtpServer = configuracion.SmtpServer;
                    }

                    if (configuracion.SmtpPort > 0)
                    {
                        smtpPort = configuracion.SmtpPort;
                    }

                    if (!string.IsNullOrWhiteSpace(configuracion.SmtpSenderName))
                    {
                        senderName = configuracion.SmtpSenderName;
                    }

                    // Solo utilizar el correo de MySQL si coincide
                    // con la cuenta que posee la contraseña configurada.
                    if (string.Equals(
                        configuracion.SmtpEmail?.Trim(),
                        _settings.SenderEmail?.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        senderEmail = configuracion.SmtpEmail.Trim();
                    }
                    else if (!string.IsNullOrWhiteSpace(configuracion.SmtpEmail))
                    {
                        _logger.LogWarning(
                            "El correo SMTP configurado en MySQL no coincide " +
                            "con la cuenta que tiene credenciales activas. " +
                            "Se mantiene el remitente anterior.");
                    }
                }

                if (string.IsNullOrWhiteSpace(smtpServer) ||
                    string.IsNullOrWhiteSpace(senderEmail) ||
                    string.IsNullOrWhiteSpace(senderPassword) ||
                    smtpPort < 1 ||
                    smtpPort > 65535)
                {
                    throw new InvalidOperationException(
                        "La configuración SMTP está incompleta.");
                }

                var email = new MimeMessage();

                email.From.Add(
                    new MailboxAddress(
                        senderName ?? "Academia de Tennis",
                        senderEmail.Trim()));

                email.To.Add(
                    MailboxAddress.Parse(destinatario.Trim()));

                email.Subject = asunto;

                email.Body = new BodyBuilder
                {
                    HtmlBody = mensaje
                }.ToMessageBody();

                using var smtp = new SmtpClient();

                smtp.Timeout = 20000;

                await smtp.ConnectAsync(
                    smtpServer.Trim(),
                    smtpPort,
                    SecureSocketOptions.StartTls);

                _logger.LogInformation(
                    "Conexión SMTP establecida correctamente.");

                await smtp.AuthenticateAsync(
                    senderEmail.Trim(),
                    senderPassword.Trim());

                _logger.LogInformation(
                    "Autenticación SMTP correcta.");

                await smtp.SendAsync(email);

                _logger.LogInformation(
                    "Correo enviado correctamente.");

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error durante el envío de correo.");

                throw;
            }
        }
    }
}