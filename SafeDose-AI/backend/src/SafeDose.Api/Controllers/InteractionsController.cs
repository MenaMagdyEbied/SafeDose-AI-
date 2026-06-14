using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers;

[ApiController]
[Route("api/interactions")]
[Authorize]
public class InteractionsController : ControllerBase
{
    private readonly CheckCatalogInteractionsUseCase _checkCatalog;
    private readonly GetInteractionHistoryUseCase _getHistory;
    private readonly GetInteractionCheckByIdUseCase _getById;
    private readonly AcknowledgeWarningUseCase _acknowledge;
    private readonly DeleteInteractionCheckUseCase _delete;

    public InteractionsController(
        CheckCatalogInteractionsUseCase checkCatalog,
        GetInteractionHistoryUseCase getHistory,
        GetInteractionCheckByIdUseCase getById,
        AcknowledgeWarningUseCase acknowledge,
        DeleteInteractionCheckUseCase delete)
    {
        _checkCatalog = checkCatalog;
        _getHistory = getHistory;
        _getById = getById;
        _acknowledge = acknowledge;
        _delete = delete;
    }

    // The ONE check endpoint. Takes catalog drug IDs (from search OR from "my meds" checkboxes).
    // Optional patientId pulls in the patient's age, allergies, chronic conditions, and active meds as context.
    [HttpPost("check")]
    public async Task<IActionResult> Check(
        [FromBody] CheckCatalogInteractionsRequestDto request,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null)
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        try
        {
            var result = await _checkCatalog.ExecuteAsync(request, accountId, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(
        [FromQuery] int patientId,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        if (patientId <= 0)
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed,
                "patientId is required"));

        var accountId = GetAccountId();
        if (accountId == null)
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        try
        {
            var page = await _getHistory.ExecuteAsync(patientId, accountId, limit, offset);
            return Ok(page);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var accountId = GetAccountId();
        if (accountId == null)
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        try
        {
            var result = await _getById.ExecuteAsync(id, accountId);
            if (result == null)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, ArabicMessages.CheckNotFound));
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
    }

    [HttpPost("{id:int}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var accountId = GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        try
        {
            var ok = await _acknowledge.ExecuteAsync(id, accountId);
            if (!ok)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, ArabicMessages.CheckNotFound));
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        try
        {
            var ok = await _delete.ExecuteAsync(id, accountId, cancellationToken);
            if (!ok)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, ArabicMessages.CheckNotFound));
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
    }

    private string? GetAccountId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("nameid")
        ?? User.FindFirstValue("uid");
}
