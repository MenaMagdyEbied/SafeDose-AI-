using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.UseCases;
using System.Security.Claims;

namespace SafeDose.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicalCardController : ControllerBase
    {
        private readonly GetPublicMedicalCardUseCase _getPublicMedicalCardUseCase;

        public MedicalCardController(GetPublicMedicalCardUseCase getPublicMedicalCardUseCase)
        {
            _getPublicMedicalCardUseCase = getPublicMedicalCardUseCase;
        }

        [AllowAnonymous]
        [HttpGet("Public/{token}")]
        public async Task<IActionResult> GetPublicCard(Guid token)
        {
            try
            {
                var cardDto = await _getPublicMedicalCardUseCase.ExecuteAsync(token);
                return Ok(new { success = true, message = "Medical card retrieved successfully.", data = cardDto });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message, data = (object?)null });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message, data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while fetching the medical card.", data = (object?)null });
            }
        }
        [HttpGet("Private/{patientId}")]
        public async Task<IActionResult> GetPrivateCard(
            int patientId, 
            [FromServices] GetPrivateMedicalCardUseCase getPrivateUseCase)
        {
            try
            {
                var accountId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(accountId)) return Unauthorized(new { success = false, message = "User not authenticated.", data = (object?)null });

                var cardDto = await getPrivateUseCase.ExecuteAsync(patientId, accountId);
                return Ok(new { success = true, message = "Medical card retrieved successfully.", data = cardDto });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message, data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while fetching the medical card.", data = (object?)null });
            }
        }

        [HttpGet("Private/{patientId}/qrcode")]
        public async Task<IActionResult> GetPrivateQrCode(
            int patientId,
            [FromServices] GenerateQrCodeUseCase generateQrCodeUseCase)
        {
            try
            {
                var accountId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(accountId)) return Unauthorized(new { success = false, message = "User not authenticated.", data = (object?)null });

                var qrCodeBytes = await generateQrCodeUseCase.ExecuteAsync(patientId, accountId);
                
                
                return File(qrCodeBytes, "image/png");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message, data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while generating the QR code.", data = (object?)null });
            }
        }

        [HttpGet("Private/{patientId}/pdf")]
        public async Task<IActionResult> GetPrivatePdf(
            int patientId,
            [FromServices] GenerateMedicalCardPdfUseCase generatePdfUseCase)
        {
            try
            {
                var accountId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(accountId)) return Unauthorized(new { success = false, message = "User not authenticated.", data = (object?)null });

                var pdfBytes = await generatePdfUseCase.ExecuteAsync(patientId, accountId);
                
                return File(pdfBytes, "application/pdf", $"MedicalCard_Patient_{patientId}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message, data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while generating the PDF.", data = (object?)null });
            }
        }
    }
}
