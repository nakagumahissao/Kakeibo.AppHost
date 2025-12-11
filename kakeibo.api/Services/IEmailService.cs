using kakeibo.api.Data;
using kakeibo.api.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string userId, string token);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ClientSettings _clientSettings;

    public EmailService(IOptions<EmailSettings> options, IClientSettings clientSettings)
    {
        _settings = options.Value;
        _clientSettings = clientSettings.GetClientSettings();
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string userId, string token)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass),
                EnableSsl = true
            };

            var resetUrl = $"{_clientSettings.ClientUrl}:{_clientSettings.ClientPort}/resetpassword/{WebUtility.UrlEncode(userId)}?token={WebUtility.UrlEncode(token)}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SmtpUser, _settings.SenderName),
                Subject = "Solicitação de Redefinição de Senha",
                Body = $@"
Olá,

Recebemos uma solicitação para redefinir sua senha.

Clique no link abaixo para criar uma nova senha:
{resetUrl}

⚠ Atenção: Este link é válido somente por 24 horas.

Se você não solicitou a redefinição de senha, apenas ignore este e-mail.

Lembrete: após redefinir sua senha, você precisará efetuar o login novamente para usar a nova senha.

Obrigado,


Equipe do {_settings.SenderName}
",
                IsBodyHtml = false
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Falha ao enviar e-mail: {ex.Message}");
            return false;
        }
    }
}
