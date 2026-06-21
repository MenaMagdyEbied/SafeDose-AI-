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
//[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly ParsePrescriptionUseCase _parseUseCase;
    private readonly SavePrescriptionUseCase _saveUseCase;
    private readonly IUserGlobalServices _userGlobalServices;

    public PrescriptionsController(
        ParsePrescriptionUseCase parseUseCase,
        SavePrescriptionUseCase saveUseCase,
        IUserGlobalServices userGlobalServices)
    {
        _parseUseCase = parseUseCase;
        _saveUseCase = saveUseCase;
        _userGlobalServices = userGlobalServices;
    }

    [HttpPost("parse")]
    public async Task<IActionResult> ParsePrescription(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

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
        if (dto == null)
        {
            return BadRequest("Invalid prescription data.");
        }

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

    [HttpGet("Patient/{patientId}/Summary")]
    public async Task<IActionResult> GetPrescriptionsSummary(int patientId, [FromServices] GetPatientPrescriptionsUseCase useCase)
    {
        try
        {
            Account account = await _userGlobalServices.GetUser();
            var result = await useCase.ExecuteAsync(patientId, account.Id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Error retrieving prescriptions: {ex.Message}" });
        }
    }

    [HttpGet("{prescriptionId}/Details")]
    public async Task<IActionResult> GetPrescriptionDetails(int prescriptionId, [FromServices] GetPrescriptionDetailsUseCase useCase)
    {
        try
        {
            Account account = await _userGlobalServices.GetUser();
            var result = await useCase.ExecuteAsync(prescriptionId, account.Id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Error retrieving prescription details: {ex.Message}" });
        }
    }
}

