using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Ingestion.Dto;
using Modules.Ingestion.Service;
using Shared.Abstractions;
using Shared.Entity;
using Shared.Enums;

namespace Modules.Ingestion.Controller;

/// <summary>
/// Ingestion controller for batch readings and run listing
/// </summary>
[ApiController]
[Route("api/v1/ingestion")]
[Tags("Ingestion")]
[Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
public class IngestionController(
    IIngestionService ingestionService,
    ITenantContext tenantContext,
    ILogger<IngestionController> logger) : ControllerBase
{
    /// <summary>
    /// Accept a batch of readings: create an ingestion run, validate and process each reading, return run ID and summary
    /// </summary>
    /// <param name="readings">Array of readings to ingest</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Run ID and summary (total, accepted, rejected, status)</returns>
    [HttpPost("readings", Name = "IngestReadingsV1")]
    [ProducesResponseType(typeof(IngestReadingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IngestReadingsResponse>> IngestReadings(
        [FromBody] List<IngestReadingItem> readings,
        CancellationToken cancellationToken = default)
    {
        var orgId = tenantContext.CurrentOrganizationId;
        if (orgId == null)
        {
            logger.LogWarning("Ingest readings called without tenant context");
            return Unauthorized(new { message = "Organization context is required" });
        }

        if (readings == null)
        {
            return BadRequest(new { message = "Readings array is required" });
        }

        try
        {
            var response = await ingestionService.IngestReadingsAsync(orgId.Value, readings, cancellationToken);
            logger.LogInformation("Ingestion run {RunId} created: {Accepted} accepted, {Rejected} rejected",
                response.RunId, response.AcceptedRecords, response.RejectedRecords);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Ingest readings validation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ingesting readings");
            return StatusCode(500, new { message = "An error occurred while ingesting readings" });
        }
    }

    /// <summary>
    /// List ingestion runs (paginated), filter by status, source, date range
    /// </summary>
    /// <param name="request">Query parameters (pagination, status, source, from, to)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of runs</returns>
    [HttpGet("runs", Name = "GetIngestionRunsV1")]
    [ProducesResponseType(typeof(PagedResponse<IngestionRunListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<IngestionRunListResponse>>> GetRuns(
        [FromQuery] GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default)
    {
        var orgId = tenantContext.CurrentOrganizationId;
        if (orgId == null)
        {
            logger.LogWarning("Get ingestion runs called without tenant context");
            return Unauthorized(new { message = "Organization context is required" });
        }

        try
        {
            var (items, totalCount) = await ingestionService.GetRunsAsync(orgId.Value, request, cancellationToken);
            logger.LogInformation("Retrieved {Count} ingestion runs", items.Count);
            return Ok(new PagedResponse<IngestionRunListResponse>
            {
                Items = items.Select(MapToListResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting ingestion runs");
            return StatusCode(500, new { message = "An error occurred while getting ingestion runs" });
        }
    }

    /// <summary>
    /// Get run details including per-reading results (accepted/rejected)
    /// </summary>
    /// <param name="id">Run ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Run with accepted readings and rejected entries</returns>
    [HttpGet("runs/{id}", Name = "GetIngestionRunByIdV1")]
    [ProducesResponseType(typeof(IngestionRunDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IngestionRunDetailResponse>> GetRunById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var orgId = tenantContext.CurrentOrganizationId;
        if (orgId == null)
        {
            logger.LogWarning("Get ingestion run called without tenant context");
            return Unauthorized(new { message = "Organization context is required" });
        }

        try
        {
            var run = await ingestionService.GetRunByIdAsync(orgId.Value, id, cancellationToken);
            logger.LogInformation("Retrieved ingestion run {Id}", id);
            return Ok(MapToDetailResponse(run));
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Ingestion run not found: {Id}", id);
            return NotFound(new { message = $"Ingestion run with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting ingestion run {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting ingestion run" });
        }
    }

    private static IngestionRunListResponse MapToListResponse(IngestionRun run)
    {
        return new IngestionRunListResponse
        {
            Id = run.Id,
            Source = run.Source,
            Status = run.Status,
            TotalRecords = run.TotalRecords,
            AcceptedRecords = run.AcceptedRecords,
            RejectedRecords = run.RejectedRecords,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            OrganizationId = run.OrganizationId,
            CreatedAt = run.CreatedAt
        };
    }

    private static IngestionRunDetailResponse MapToDetailResponse(IngestionRun run)
    {
        return new IngestionRunDetailResponse
        {
            Id = run.Id,
            Source = run.Source,
            Status = run.Status,
            TotalRecords = run.TotalRecords,
            AcceptedRecords = run.AcceptedRecords,
            RejectedRecords = run.RejectedRecords,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            OrganizationId = run.OrganizationId,
            CreatedAt = run.CreatedAt,
            AcceptedReadings = run.SensorReadings.Select(r => new IngestionReadingResult
            {
                Id = r.Id,
                SensorId = r.SensorId,
                Value = r.Value,
                TimestampUtc = r.TimestampUtc,
                Unit = r.Unit.ToString()
            }).ToList(),
            RejectedReadings = run.RejectedReadings.Select(r => new RejectedReadingResult
            {
                RowIndex = r.RowIndex,
                RejectionReason = r.RejectionReason
            }).ToList()
        };
    }
}
