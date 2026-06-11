using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases.Medication;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers;

// Module 4 — Medication Management.
// Ahmed owns this. Every endpoint requires JWT + enforces ownership.
[ApiController]
[Route("api/medications")]
[Authorize]
public class MedicationsController : ControllerBase
{
    private readonly AddMedicationManuallyUseCase _addManual;
    private readonly AddMedicationsFromPrescriptionUseCase _addBulk;
    private readonly UpdateMedicationUseCase _update;
    private readonly ChangeMedicationStatusUseCase _status;
    private readonly GetActiveMedicationsUseCase _getActive;
    private readonly GetMedicationHistoryUseCase _getHistory;
    private readonly GetMedicationByIdUseCase _getById;

    public MedicationsController(
        AddMedicationManuallyUseCase addManual,
        AddMedicationsFromPrescriptionUseCase addBulk,
        UpdateMedicationUseCase update,
        ChangeMedicationStatusUseCase status,
        GetActiveMedicationsUseCase getActive,
        GetMedicationHistoryUseCase getHistory,
        GetMedicationByIdUseCase getById)
    {
        _addManual = addManual;
        _addBulk = addBulk;
        _update = update;
        _status = status;
        _getActive = getActive;
        _getHistory = getHistory;
        _getById = getById;
    }

    // POST /api/medications — add a single manual medication
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] AddMedicationDto dto,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var result = await _addManual.ExecuteAsync(accountId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.PatientMedicationId }, result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadValidation(ex.Message); }
    }

    // POST /api/medications/from-prescription — bulk add from confirmed OCR
    [HttpPost("from-prescription")]
    public async Task<IActionResult> AddFromPrescription(
        [FromBody] BulkAddFromPrescriptionDto dto,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var inserted = await _addBulk.ExecuteAsync(accountId, dto, cancellationToken);
            return Ok(new { insertedCount = inserted });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadValidation(ex.Message); }
    }

    // GET /api/medications/patient/{patientId} — active meds for a patient
    [HttpGet("patient/{patientId:int}")]
    public async Task<IActionResult> GetActiveForPatient(int patientId)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var list = await _getActive.ExecuteAsync(patientId, accountId);
            return Ok(list);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // GET /api/medications/patient/{patientId}/history — grouped by status
    [HttpGet("patient/{patientId:int}/history")]
    public async Task<IActionResult> GetHistory(int patientId)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var history = await _getHistory.ExecuteAsync(patientId, accountId);
            if (history == null)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, ArabicMessages.PatientNotFound));
            return Ok(history);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // GET /api/medications/{id} — single med detail
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var med = await _getById.ExecuteAsync(id, accountId);
            if (med == null)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, "الدواء غير موجود"));
            return Ok(med);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // PUT /api/medications/{id} — update dose/freq/timing
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMedicationDto dto,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var result = await _update.ExecuteAsync(id, accountId, dto, cancellationToken);
            if (result == null)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, "الدواء غير موجود"));
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadValidation(ex.Message); }
        catch (ArgumentException ex) { return BadValidation(ex.Message); }
    }

    // POST /api/medications/{id}/pause
    [HttpPost("{id:int}/pause")]
    public async Task<IActionResult> Pause(int id, CancellationToken ct) =>
        await StatusChange(id, (cid, accId, c) => _status.PauseAsync(cid, accId, c), ct);

    // POST /api/medications/{id}/resume
    [HttpPost("{id:int}/resume")]
    public async Task<IActionResult> Resume(int id, CancellationToken ct) =>
        await StatusChange(id, (cid, accId, c) => _status.ResumeAsync(cid, accId, c), ct);

    // POST /api/medications/{id}/stop
    [HttpPost("{id:int}/stop")]
    public async Task<IActionResult> Stop(int id, CancellationToken ct) =>
        await StatusChange(id, (cid, accId, c) => _status.StopAsync(cid, accId, c), ct);

    // ─── helpers ───────────────────────────────────────────────
    private async Task<IActionResult> StatusChange(
        int id,
        Func<int, string, CancellationToken, Task<bool>> op,
        CancellationToken ct)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var ok = await op(id, accountId, ct);
            if (!ok)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, "الدواء غير موجود"));
            return NoContent();
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadValidation(ex.Message); }
    }

    private string? GetAccountId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? User.FindFirstValue("uid");

    private IActionResult Unauth() =>
        Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

    private new IActionResult Forbid() =>
        StatusCode(403, new ErrorResponse(ErrorCodes.Forbidden, ArabicMessages.Forbidden));

    private IActionResult BadValidation(string message) =>
        BadRequest(new ErrorResponse(
            ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, message));
}
