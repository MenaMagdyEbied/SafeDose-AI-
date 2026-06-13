using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.UseCases;

namespace SafeDose.Api.Controllers;

// Powers the autocomplete search box for the Egyptian drug catalog (24,892 rows).
[ApiController]
[Route("api/drugs")]
public class DrugsController : ControllerBase
{
    private readonly SearchDrugsUseCase _searchDrugs;

    public DrugsController(SearchDrugsUseCase searchDrugs)
    {
        _searchDrugs = searchDrugs;
    }

    // Returns top matches for autocomplete dropdown.
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<object>());

        var results = await _searchDrugs.ExecuteAsync(q, limit);
        return Ok(results);
    }
}
