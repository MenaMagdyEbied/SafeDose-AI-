using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.UseCases;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/critical-pairs")]
[Authorize(Roles = "Admin")]
public class CriticalPairsAdminController : ControllerBase
{
    private readonly SeedCriticalPairsUseCase _seed;

    public CriticalPairsAdminController(SeedCriticalPairsUseCase seed)
    {
        _seed = seed;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized(new ErrorResponse(
                ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));

        var inserted = await _seed.ExecuteAsync(accountId, cancellationToken);
        return Ok(new { inserted });
    }
}
