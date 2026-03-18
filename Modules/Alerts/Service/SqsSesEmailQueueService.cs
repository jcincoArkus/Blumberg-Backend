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
        int Total,
        int Processed,
        int Failed,
        IReadOnlyList<string>? Errors,
        string? Error
    );

    public async Task<QueueEmailResult> QueueEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var queueUrl = RequireEnv("WORKER_QUEUE_URL");

        // Usamos únicamente SMTP_FROM_ADDRESS como remitente (SES_from_email no aplica).
        var fromEmail = RequireEnv("SMTP_FROM_ADDRESS");

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

        var fromEmail = (Environment.GetEnvironmentVariable("SMTP_FROM_ADDRESS") ?? "").Trim();
        if (string.IsNullOrWhiteSpace(fromEmail) || string.Equals(fromEmail, "TO_BE_DEFINED", StringComparison.OrdinalIgnoreCase))
            fromEmail = "noreply@arkusnexusbootcamp.com";

        if (maxMessages <= 0)
            throw new InvalidOperationException("maxMessages must be > 0");

        // Requisito: clientes sin parámetros
        using var sqs = new AmazonSQSClient();
        using var ses = new AmazonSimpleEmailServiceClient();

        var total = 0;
        var processed = 0;
        var failed = 0;
        var errors = new List<string>();

        // SQS suele limitar MaxNumberOfMessages a 10
        var batchSize = Math.Min(10, maxMessages);

        ReceiveMessageResponse receiveResponse;
        try
        {
            receiveResponse = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = batchSize,
                WaitTimeSeconds = 0,
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to receive messages from SQS. queueUrl={QueueUrl}", queueUrl);
            return new ProcessQueueResult(
                Total: 0,
                Processed: 0,
                Failed: 0,
                Errors: null,
                Error: $"SQS ReceiveMessageAsync failed: {ex.Message}"
            );
        }

        var messages = receiveResponse.Messages ?? new List<Message>();
        total = messages.Count;

        if (messages.Count == 0)
        {
            return new ProcessQueueResult(
                Total: 0,
                Processed: 0,
                Failed: 0,
                Errors: null,
                Error: null
            );
        }

        foreach (var msg in messages)
        {
            try
            {
                var rawBody = msg.Body;
                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    errors.Add("Mensaje sin body");
                    await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, cancellationToken);
                    continue;
                }

                JsonElement body;
                try
                {
                    body = JsonSerializer.Deserialize<JsonElement>(rawBody);
                }
                catch (JsonException)
                {
                    errors.Add("Mensaje con body no-JSON válido");
                    await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, cancellationToken);
                    continue;
                }

                // Soportar formato directo { "email": "..." } o wrapper tipo SNS { "Message": "{\"email\":\"...\"}" }
                string? email = null;
                if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("email", out var e))
                    email = e.GetString()?.Trim();
                else if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("Message", out var msgProp))
                {
                    var inner = msgProp.GetString();
                    if (!string.IsNullOrWhiteSpace(inner))
                    {
                        try
                        {
                            var innerJson = JsonSerializer.Deserialize<JsonElement>(inner);
                            if (innerJson.ValueKind == JsonValueKind.Object &&
                                innerJson.TryGetProperty("email", out var innerE))
                                email = innerE.GetString()?.Trim();
                        }
                        catch
                        {
                            // ignore parsing error; handled below as missing email
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add("Mensaje sin campo 'email' en el body");
                    await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, cancellationToken);
                    continue;
                }

                // Validación tipo ejemplo: requiere @ y un '.' después
                if (!email.Contains('@') ||
                    email.IndexOf('@') == email.Length - 1 ||
                    !email.Contains('.'))
                {
                    errors.Add($"Email inválido (falta @dominio): {email}");
                    await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, cancellationToken);
                    continue;
                }

                // Envío SES mínimo (SendEmailRequest).
                var subject = "Email from Blumberg queue";
                var bodyText = $"Your email {email} was processed from the queue at {DateTime.UtcNow:O}";

                await ses.SendEmailAsync(new Amazon.SimpleEmail.Model.SendEmailRequest
                {
                    Source = fromEmail,
                    Destination = new Amazon.SimpleEmail.Model.Destination
                    {
                        ToAddresses = new List<string> { email }
                    },
                    Message = new Amazon.SimpleEmail.Model.Message
                    {
                        Subject = new Amazon.SimpleEmail.Model.Content { Data = subject },
                        Body = new Amazon.SimpleEmail.Model.Body
                        {
                            Text = new Amazon.SimpleEmail.Model.Content { Data = bodyText }
                        }
                    }
                }, cancellationToken);

                // Si SES fue OK, borramos el mensaje procesado
                await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, cancellationToken);

                processed++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing one SQS message. messageId={MessageId}", msg.MessageId);
                // No borramos el mensaje si falla el envío para permitir reintento (según DLQ/visibility timeout del queue).
                errors.Add(ex.Message);
                failed++;
            }
        }

        return new ProcessQueueResult(
            Total: total,
            Processed: processed,
            Failed: failed,
            Errors: errors.Count > 0 ? errors : null,
            Error: null
        );
    }

    private static string RequireEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Required environment variable '{key}' is not set.");
        return value;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Invalid email: empty.");

        _ = new MailAddress(email);
    }
}

