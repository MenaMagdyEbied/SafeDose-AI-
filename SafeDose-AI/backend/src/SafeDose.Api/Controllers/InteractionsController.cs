using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers;

// Drug Interaction Checker — the hero feature.
// All endpoints match Mina's UI design.
[ApiController]
[Route("api/interactions")]
public class InteractionsController : ControllerBase
{
    private readonly CheckDrugInteractionUseCase _checkInteraction;
    private readonly CheckStandaloneInteractionUseCase _checkStandalone;
    private readonly GetInteractionHistoryUseCase _getHistory;
    private readonly GetInteractionCheckByIdUseCase _getById;
    private readonly AcknowledgeWarningUseCase _acknowledge;
    private readonly DeleteInteractionCheckUseCase _delete;

    public InteractionsController(
        CheckDrugInteractionUseCase checkInteraction,
        CheckStandaloneInteractionUseCase checkStandalone,
        GetInteractionHistoryUseCase getHistory,
        GetInteractionCheckByIdUseCase getById,
        AcknowledgeWarningUseCase acknowledge,
        DeleteInteractionCheckUseCase delete)
    {
        _checkInteraction = checkInteraction;
        _checkStandalone = checkStandalone;
        _getHistory = getHistory;
        _getById = getById;
        _acknowledge = acknowledge;
        _delete = delete;
    }

    // POST /api/interactions/check
    // The main "افحص التداخلات الدوائية الآن" button.
    [HttpPost("check")]
    public async Task<IActionResult> Check(
        [FromBody] CheckInteractionsRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checkInteraction.ExecuteAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Maximum"))
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.TooManyDrugs, ArabicMessages.TooManyDrugs, ex.Message));
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.DrugNotFound, ArabicMessages.DrugNotFound, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    // POST /api/interactions/check-standalone
    // Quick 2-drug check without patient context.
    [HttpPost("check-standalone")]
    public async Task<IActionResult> CheckStandalone(
        [FromQuery] int drugIdA,
        [FromQuery] int drugIdB,
        CancellationToken cancellationToken)
    {
        if (drugIdA <= 0 || drugIdB <= 0)
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed,
                "Both drugIdA and drugIdB are required"));

        try
        {
            var result = await _checkStandalone.ExecuteAsync(drugIdA, drugIdB, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    // GET /api/interactions/history?patientId=7&limit=20&offset=0
    // Returns paginated history for the Settings → History screen.
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

        var page = await _getHistory.ExecuteAsync(patientId, limit, offset);
        return Ok(page);
    }

    // GET /api/interactions/{id}
    // Full detail view for a single past check.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _getById.ExecuteAsync(id);
        if (result == null)
            return NotFound(new ErrorResponse(
                ErrorCodes.NotFound, ArabicMessages.CheckNotFound));
        return Ok(result);
    }

    // POST /api/interactions/{id}/acknowledge
    // Patient explicitly acknowledges a Level 3 warning ("I will consult my doctor").
    // Used by the UI when patient still wants to save a drug despite the danger flag.
    [HttpPost("{id:int}/acknowledge")]
    [Authorize]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        var ok = await _acknowledge.ExecuteAsync(id, accountId);
        if (!ok)
            return NotFound(new ErrorResponse(
                ErrorCodes.NotFound, ArabicMessages.CheckNotFound));
        return NoContent();
    }

    // DELETE /api/interactions/{id}
    // Soft delete — record stays in DB for compliance retention.
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        var ok = await _delete.ExecuteAsync(id, accountId, cancellationToken);
        if (!ok)
            return NotFound(new ErrorResponse(
                ErrorCodes.NotFound, ArabicMessages.CheckNotFound));
        return NoContent();
    }
}
