using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.UseCases;

namespace SafeDose.Api.Controllers;

[ApiController]
[Route("api/interactions")]
public class InteractionsController : ControllerBase
{
    private readonly CheckDrugInteractionUseCase _useCase;

    public InteractionsController(CheckDrugInteractionUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] CheckInteractionRequest request)
    {
        var result = await _useCase.ExecuteAsync(request.PatientId, request.Drugs);
        return Ok(result);
    }
}

public record CheckInteractionRequest(int PatientId, string[] Drugs);
