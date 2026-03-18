using Amazon.SimpleEmail;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Text.Json;

namespace Modules.Alerts.Service;

/// <summary>
/// Queue de emails usando SQS como buffer y envío con SES.
/// No usa Secrets Manager SDK: solo lee variables de entorno.
/// </summary>
public class SqsSesEmailQueueService(ILogger<SqsSesEmailQueueService> logger)
{
    public sealed record QueueEmailResult(string MessageId, string QueueUrl);

    public sealed record ProcessQueueResult(
        int Received,
        int Sent,
        int Deleted
    );

    public async Task<QueueEmailResult> QueueEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var queueUrl = RequireEnv("WORKER_QUEUE_URL");

        // SES_FROM_EMAIL o fallback a SMTP_FROM_ADDRESS
        var fromEmail = GetEnv("SES_FROM_EMAIL") ?? GetEnv("SMTP_FROM_ADDRESS");
        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("Missing env var: SES_FROM_EMAIL (or fallback SMTP_FROM_ADDRESS).");

        // Validación básica de formato para evitar encolar basura
        ValidateEmail(email);

        // Body simple: {"email":"..."}
        var body = JsonSerializer.Serialize(new { email });

        // Requisito: crear clientes sin parámetros (usan credenciales del entorno/Task Role)
        using var sqs = new AmazonSQSClient();

        var sendResponse = await sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = body
        }, cancellationToken);

        logger.LogInformation("Enqueued email to SQS queue. to={Email} messageId={MessageId}", email, sendResponse.MessageId);
        return new QueueEmailResult(sendResponse.MessageId, queueUrl);
    }

    public async Task<ProcessQueueResult> ProcessQueueAsync(int maxMessages = 10, CancellationToken cancellationToken = default)
    {
        var queueUrl = RequireEnv("WORKER_QUEUE_URL");

        var fromEmail = GetEnv("SES_FROM_EMAIL") ?? GetEnv("SMTP_FROM_ADDRESS");
        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("Missing env var: SES_FROM_EMAIL (or fallback SMTP_FROM_ADDRESS).");

        if (maxMessages <= 0)
            throw new InvalidOperationException("maxMessages must be > 0");

        // Requisito: clientes sin parámetros
        using var sqs = new AmazonSQSClient();
        using var ses = new AmazonSimpleEmailServiceClient();

        var received = 0;
        var sent = 0;
        var deleted = 0;

        // SQS suele limitar MaxNumberOfMessages a 10
        var batchSize = Math.Min(10, maxMessages);

        var receiveResponse = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = batchSize,
            WaitTimeSeconds = 10, // long polling
            MessageAttributeNames = ["All"],
        }, cancellationToken);

        if (receiveResponse.Messages is null || receiveResponse.Messages.Count == 0)
        {
            return new ProcessQueueResult(0, 0, 0);
        }

        received = receiveResponse.Messages.Count;

        foreach (var msg in receiveResponse.Messages)
        {
            try
            {
                var toEmail = ExtractEmail(msg.Body);
                ValidateEmail(toEmail);

                // Envío SES mínimo (SendEmailRequest).
                // Si luego quieren plantillas, se puede extender a SendTemplatedEmail.
                var subject = "Blumberg email notification";
                var bodyText = "Recibimos tu solicitud y la procesamos correctamente.";

                await ses.SendEmailAsync(new Amazon.SimpleEmail.Model.SendEmailRequest
                {
                    Source = fromEmail,
                    Destination = new Amazon.SimpleEmail.Model.Destination
                    {
                        ToAddresses = [toEmail]
                    },
                    Message = new Amazon.SimpleEmail.Model.Message
                    {
                        Subject = new Amazon.SimpleEmail.Model.Content(subject),
                        Body = new Amazon.SimpleEmail.Model.Body
                        {
                            Text = new Amazon.SimpleEmail.Model.Content(bodyText)
                        }
                    }
                }, cancellationToken);

                sent++;

                // Borra el mensaje procesado
                await sqs.DeleteMessageAsync(new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = msg.ReceiptHandle
                }, cancellationToken);

                deleted++;
            }
            catch (Exception ex)
            {
                // No borramos el mensaje si falla el envío para permitir reintento (según DLQ/visibility timeout del queue).
                logger.LogError(ex, "Failed processing one SQS message. messageId={MessageId}", msg.MessageId);
            }
        }

        return new ProcessQueueResult(received, sent, deleted);
    }

    private static string ExtractEmail(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "";

        body = body.Trim();

        // Si viene como JSON: {"email":"..."}
        if (body.StartsWith("{", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("email", out var emailEl))
                    return emailEl.GetString() ?? "";
            }
            catch
            {
                // Fall back to raw parsing below
            }
        }

        // Fallback: body = email directo (o string con comillas)
        if (body.StartsWith("\"") && body.EndsWith("\"") && body.Length >= 2)
            body = body[1..^1];

        return body;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Invalid email: empty.");

        // MailAddress lanza excepción si no es válido
        _ = new MailAddress(email);
    }

    private static string RequireEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Required environment variable '{key}' is not set.");
        return value;
    }

    private static string? GetEnv(string key) => Environment.GetEnvironmentVariable(key);
}

