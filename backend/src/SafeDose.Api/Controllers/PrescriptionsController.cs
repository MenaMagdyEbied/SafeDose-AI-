using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.UseCases;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly ParsePrescriptionUseCase _parseUseCase;
    private readonly SavePrescriptionUseCase _saveUseCase;
    private readonly GetPatientPrescriptionsUseCase _listUseCase;
    private readonly GetPrescriptionDetailsUseCase _detailsUseCase;
    private readonly DeletePrescriptionUseCase _deleteUseCase;
    private readonly IUserGlobalServices _userGlobalServices;

    public PrescriptionsController(
        ParsePrescriptionUseCase parseUseCase,
        SavePrescriptionUseCase saveUseCase,
        GetPatientPrescriptionsUseCase listUseCase,
        GetPrescriptionDetailsUseCase detailsUseCase,
        DeletePrescriptionUseCase deleteUseCase,
        IUserGlobalServices userGlobalServices)
    {
        _parseUseCase = parseUseCase;
        _saveUseCase = saveUseCase;
        _listUseCase = listUseCase;
        _detailsUseCase = detailsUseCase;
        _deleteUseCase = deleteUseCase;
        _userGlobalServices = userGlobalServices;
    }

    [HttpPost("parse")]
    public async Task<IActionResult> ParsePrescription(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            Account account = await _userGlobalServices.GetUser();
            using var stream = file.OpenReadStream();
            var result = await _parseUseCase.ExecuteAsync(stream, file.FileName, file.ContentType, account.Id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing prescription with AI: {ex.Message}");
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SavePrescription([FromBody] SavePrescriptionDto dto)
    {
        if (dto == null) return BadRequest("Invalid prescription data.");

        try
        {
            Account account = await _userGlobalServices.GetUser();
            var prescriptionId = await _saveUseCase.ExecuteAsync(dto, account.Id);
            return Ok(new { PrescriptionId = prescriptionId, Message = "Prescription saved successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving prescription: {ex.Message}");
        }
    }

    // GET /api/Prescriptions/Patient/{patientId}/Summary — list view for the FE.
    [HttpGet("Patient/{patientId:int}/Summary")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        try
        {
            var accountId = await GetAccountIdAsync();
            var list = await _listUseCase.ExecuteAsync(patientId, accountId);
            return Ok(new { success = true, data = list });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
    }

    // GET /api/Prescriptions/{id}/Details — single prescription with its drugs.
    [HttpGet("{prescriptionId:int}/Details")]
    public async Task<IActionResult> GetDetails(int prescriptionId)
    {
        try
        {
            var accountId = await GetAccountIdAsync();
            var dto = await _detailsUseCase.ExecuteAsync(prescriptionId, accountId);
            return Ok(new { success = true, data = dto });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
    }

    // DELETE /api/Prescriptions/{id} — removes the prescription and its drugs.
    [HttpDelete("{prescriptionId:int}")]
    public async Task<IActionResult> Delete(int prescriptionId)
    {
        try
        {
            var accountId = await GetAccountIdAsync();
            var removed = await _deleteUseCase.ExecuteAsync(prescriptionId, accountId);
            if (!removed) return NotFound(new { success = false, message = "Prescription not found." });
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
    }

    private async Task<string> GetAccountIdAsync()
    {
        var fromClaims = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("nameid")
                       ?? User.FindFirstValue("uid");
        if (!string.IsNullOrWhiteSpace(fromClaims)) return fromClaims!;

        var account = await _userGlobalServices.GetUser();
        return account.Id;
    }
}
