using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers;

// Every endpoint requires a valid JWT from Module 1 (Auth)
// Patients are ALWAYS scoped to the calling Account
[ApiController]
[Route("api/patients")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly CreatePatientUseCase _create;
    private readonly UpdatePatientUseCase _update;
    private readonly GetMyPatientsUseCase _getMine;
    private readonly GetPatientByIdUseCase _getById;
    private readonly DeactivatePatientUseCase _deactivate;

    public PatientsController(
        CreatePatientUseCase create,
        UpdatePatientUseCase update,
        GetMyPatientsUseCase getMine,
        GetPatientByIdUseCase getById,
        DeactivatePatientUseCase deactivate)
    {
        _create = create;
        _update = update;
        _getMine = getMine;
        _getById = getById;
        _deactivate = deactivate;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientDto dto,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var result = await _create.ExecuteAsync(accountId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.PatientId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] bool includeInactive = false)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        var list = await _getMine.ExecuteAsync(accountId, includeInactive);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var result = await _getById.ExecuteAsync(id, accountId);
            if (result == null)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, ArabicMessages.PatientNotFound));
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePatientDto dto,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var result = await _update.ExecuteAsync(id, accountId, dto, cancellationToken);
            if (result == null)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, ArabicMessages.PatientNotFound));
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        var ok = await _deactivate.ExecuteAsync(id, accountId, cancellationToken);
        if (!ok)
            return NotFound(new ErrorResponse(
                ErrorCodes.NotFound, ArabicMessages.PatientNotFound));
        return NoContent();
    }

    private string? GetAccountId()
    {
        // Identity sets the user id as NameIdentifier claim 
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("uid");
    }

    private IActionResult Unauth() =>
        Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));
}
