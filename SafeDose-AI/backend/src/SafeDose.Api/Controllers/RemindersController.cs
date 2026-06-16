using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases.Reminders;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers;

[ApiController]
[Route("api/reminders")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly SetMedicationTimesUseCase _setTimes;
    private readonly GetDueRemindersUseCase _getDue;
    private readonly RecordReminderResponseUseCase _record;

    public RemindersController(
        SetMedicationTimesUseCase setTimes,
        GetDueRemindersUseCase getDue,
        RecordReminderResponseUseCase record)
    {
        _setTimes = setTimes;
        _getDue = getDue;
        _record = record;
    }

    // Patient (or frontend on Save medication) sets the times for a medication.
    // Replaces all existing times for that medication.
    [HttpPut("times")]
    public async Task<IActionResult> SetTimes([FromBody] SetMedicationTimesDto dto)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            await _setTimes.ExecuteAsync(accountId, dto);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    // The notification dispatcher polls this every minute.
    // ?from=2026-06-16T08:00:00Z&to=2026-06-16T08:01:00Z
    [HttpGet("due")]
    public async Task<IActionResult> Due([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var due = await _getDue.ExecuteAsync(accountId, from, to);
            return Ok(due);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    // Patient tapped a button on the notification.
    [HttpPost("respond")]
    public async Task<IActionResult> Respond([FromBody] RecordReminderResponseDto dto)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var id = await _record.ExecuteAsync(accountId, dto);
            return Ok(new { reminderResponseId = id });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    private string? GetAccountId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("nameid")
        ?? User.FindFirstValue("uid");

    private IActionResult Unauth() =>
        Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));
}
