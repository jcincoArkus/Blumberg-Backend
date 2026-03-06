using System.Net;
using System.Net.Mail;
using Adapters.Config;
using Microsoft.Extensions.Logging;
using Shared.Notifications;

namespace Adapters.Email;

/// <summary>
/// Sends alert notifications via SMTP. Uses EmailConfig (SMTP host, port, from, optional credentials).
/// </summary>
public class SmtpAlertNotificationSender(EmailConfig emailConfig, ILogger<SmtpAlertNotificationSender> logger) : IAlertNotificationSender
{
    /// <inheritdoc />
    public async Task SendAsync(AlertNotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (message.RecipientEmails.Count == 0)
        {
            logger.LogDebug("No recipients for alert {AlertId}, skipping send", message.AlertId);
            return;
        }

        var subject = $"Alert: {message.Severity} - {message.EquipmentName} / {message.SensorName}";
        var body = $@"Alert triggered at {message.TriggeredAtUtc:yyyy-MM-dd HH:mm:ss} UTC.

Severity: {message.Severity}
Equipment: {message.EquipmentName}
Sensor: {message.SensorName}
Detected value: {message.DetectedValue}
Threshold: {message.ThresholdMin} - {message.ThresholdMax}

Alert ID: {message.AlertId}";

        using var smtp = new SmtpClient(emailConfig.SmtpHost, emailConfig.SmtpPort)
        {
            EnableSsl = emailConfig.SmtpUseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        if (!string.IsNullOrWhiteSpace(emailConfig.SmtpUser))
            smtp.Credentials = new NetworkCredential(emailConfig.SmtpUser, emailConfig.SmtpPassword);

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailConfig.SmtpFromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        foreach (var to in message.RecipientEmails)
            mailMessage.To.Add(to);

        try
        {
            await smtp.SendMailAsync(mailMessage, cancellationToken);
            logger.LogInformation("Alert email sent for alert {AlertId} to {Count} recipient(s)", message.AlertId, message.RecipientEmails.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send alert email for alert {AlertId}", message.AlertId);
            throw;
        }
    }
}
