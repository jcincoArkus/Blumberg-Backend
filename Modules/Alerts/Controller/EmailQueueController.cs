using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Service;

namespace Modules.Alerts.Controller;

[ApiController]
[Route("api")]
[Tags("EmailQueue")]
[AllowAnonymous]
public class EmailQueueController(SqsSesEmailQueueService service, ILogger<EmailQueueController> logger) : ControllerBase
{
    public sealed record QueueEmailRequest(string Email);

    public sealed record QueueEmailResponse(string MessageId, string QueueUrl);

    public sealed record ProcessQueueRequest(int? MaxMessages);

    public sealed record ProcessQueueResponse(
        bool Ok,
        int Processed,
        int Total,
        IReadOnlyList<string>? Errors
    );

    /// <summary>
    /// Encola un email en SQS.
    /// </summary>
    [HttpPost("queue-email", Name = "QueueEmail")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(QueueEmailResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueueEmailResponse>> QueueEmail(
        [FromBody] QueueEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        // Nota: validación básica dentro del servicio
        logger.LogInformation("QueueEmail requested. email={Email}", request.Email);
        var result = await service.QueueEmailAsync(request.Email, cancellationToken);
        return Ok(new QueueEmailResponse(result.MessageId, result.QueueUrl));
    }

    /// <summary>
    /// Procesa mensajes pendientes de SQS: envía correos con SES y borra los mensajes procesados.
    /// </summary>
    [HttpPost("process-queue", Name = "ProcessQueue")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProcessQueueResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessQueueResponse>> ProcessQueue(
        [FromBody] ProcessQueueRequest? request,
        CancellationToken cancellationToken = default)
    {
        var maxMessages = request?.MaxMessages ?? 10;
        logger.LogInformation("ProcessQueue requested. maxMessages={MaxMessages}", maxMessages);

        try
        {
            var result = await service.ProcessQueueAsync(maxMessages, cancellationToken);

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    ok = false,
                    processed = result.Processed,
                    total = result.Total,
                    errors = (IReadOnlyList<string>?)null,
                    error = result.Error
                });
            }

            var errors = (result.Errors != null && result.Errors.Count > 0) ? result.Errors : null;

            return Ok(new ProcessQueueResponse(
                Ok: true,
                Processed: result.Processed,
                Total: result.Total,
                Errors: errors
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                ok = false,
                processed = 0,
                total = 0,
                errors = (IReadOnlyList<string>?)null,
                error = ex.Message
            });
        }
    }
}

