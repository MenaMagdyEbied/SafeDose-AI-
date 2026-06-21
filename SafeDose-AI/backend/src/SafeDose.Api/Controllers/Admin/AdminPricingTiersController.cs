using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.UseCases.Admin.PricingTiers;

namespace SafeDose.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/pricing-tiers")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AdminPricingTiersController : ControllerBase
{
    private readonly GetAdminPricingTiersUseCase   _get;
    private readonly UpdatePricingTierAdminUseCase _update;
    private readonly AddFeatureUseCase             _addFeature;
    private readonly RemoveFeatureUseCase          _removeFeature;

    public AdminPricingTiersController(
        GetAdminPricingTiersUseCase get,
        UpdatePricingTierAdminUseCase update,
        AddFeatureUseCase addFeature,
        RemoveFeatureUseCase removeFeature)
    {
        _get           = get;
        _update        = update;
        _addFeature    = addFeature;
        _removeFeature = removeFeature;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _get.ExecuteAsync());

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAdminPricingTierDto dto)
    {
        try
        {
            var updated = await _update.ExecuteAsync(id, dto);
            if (updated == null) return NotFound(new { code = "TIER_NOT_FOUND" });
            return Ok(updated);
        }
        catch (ArgumentException ex) { return BadRequest(new { code = "VALIDATION", message = ex.Message }); }
    }

    [HttpPost("{id:int}/features")]
    public async Task<IActionResult> AddFeature(int id, [FromBody] AddFeatureDto dto)
    {
        try
        {
            var newId = await _addFeature.ExecuteAsync(id, dto.LabelArabic);
            return CreatedAtAction(nameof(List), null, new { featureId = newId });
        }
        catch (ArgumentException ex)         { return BadRequest(new { code = "VALIDATION", message = ex.Message }); }
        catch (InvalidOperationException)    { return NotFound(new { code = "TIER_NOT_FOUND" }); }
    }

    [HttpDelete("{id:int}/features/{featureId:int}")]
    public async Task<IActionResult> RemoveFeature(int id, int featureId)
    {
        var removed = await _removeFeature.ExecuteAsync(featureId);
        return removed ? NoContent() : NotFound(new { code = "FEATURE_NOT_FOUND" });
    }
}
