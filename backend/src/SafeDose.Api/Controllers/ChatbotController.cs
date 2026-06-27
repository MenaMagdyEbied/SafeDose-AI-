using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Api.Auth;
using SafeDose.Application.UseCases.Chatbot;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers;

// Called by the Langflow chatbot. Two variants:
//
//   GET /api/chatbot/context              — user JWT (Authorization: Bearer ...)
//                                            used when the Angular frontend forwards the user's token
//
//   GET /api/chatbot/service-context?patientId=42  — service token (X-Service-Token: ...)
//                                            used when Langflow itself calls (server-to-server)
//                                            no user JWT involved; patientId is required
//
// Both return the same shape, embedded verbatim into the chatbot prompt.
[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly GetChatbotPatientContextUseCase   _context;
    private readonly ProcessChatMessageUseCase         _chat;
    private readonly ProcessPublicChatMessageUseCase   _publicChat;

    public ChatbotController(
        GetChatbotPatientContextUseCase context,
        ProcessChatMessageUseCase chat,
        ProcessPublicChatMessageUseCase publicChat)
    {
        _context    = context;
        _chat       = chat;
        _publicChat = publicChat;
    }

    // ── Authenticated chat ──────────────────────────────────────────────────
    // PatientId is REQUIRED — family accounts may manage multiple patients.
    [HttpPost("chat")]
    [Authorize]
    public async Task<IActionResult> Chat(
        [FromBody] SafeDose.Application.DTOs.Chatbot.ChatRequestDto req,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null)
            return Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        try
        {
            var result = await _chat.ExecuteAsync(accountId, req, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, "البيانات المدخلة غير صحيحة", ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, "المريض ده مش تابع لحسابك"));
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new ErrorResponse(
                "LLM_UNAVAILABLE",
                "حدث خطأ في الاتصال بالمساعد، حاول مرة أخرى",
                ex.Message));
        }
    }

    // ── Anonymous chat (no login) ───────────────────────────────────────────
    // Only answers general drug-info questions from the Egyptian CSV catalog.
    // Refuses symptom analysis (no patient context).
    [HttpPost("chat-public")]
    [AllowAnonymous]
    public async Task<IActionResult> ChatPublic(
        [FromBody] SafeDose.Application.DTOs.Chatbot.PublicChatRequestDto req,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _publicChat.ExecuteAsync(req, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, "البيانات المدخلة غير صحيحة", ex.Message));
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new ErrorResponse(
                "LLM_UNAVAILABLE",
                "حدث خطأ في الاتصال بالمساعد، حاول مرة أخرى",
                ex.Message));
        }
    }

    // ── User-auth variant (frontend forwards JWT) ──────────────────────────
    [HttpGet("context")]
    [Authorize]
    public async Task<IActionResult> Context(
        [FromQuery] int? patientId,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null)
            return Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        var result = await _context.ExecuteAsync(accountId, patientId, cancellationToken);
        if (result == null)
            return NotFound(new ErrorResponse(ErrorCodes.NotFound, ArabicMessages.PatientNotFound));

        return Ok(result);
    }

    // ── Service-token variant (Langflow uses a fixed key, no user JWT) ─────
    // PatientId is required because there's no user identity to derive it from.
    [HttpGet("service-context")]
    [ServiceToken]
    public async Task<IActionResult> ServiceContext(
        [FromQuery] int patientId,
        CancellationToken cancellationToken)
    {
        if (patientId <= 0)
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed,
                "يجب تحديد معرف المريض"));

        // accountId is fetched from the patient (no ownership check — Langflow is trusted).
        // Pass empty string to bypass the ownership guard, then re-derive accountId for the lookup.
        var dto = await _context.ExecuteForServiceAsync(patientId, cancellationToken);
        if (dto == null)
            return NotFound(new ErrorResponse(ErrorCodes.NotFound, ArabicMessages.PatientNotFound));

        return Ok(dto);
    }

    private string? GetAccountId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("nameid")
        ?? User.FindFirstValue("uid");
}
