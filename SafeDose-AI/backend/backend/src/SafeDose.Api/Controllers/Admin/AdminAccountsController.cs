using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.UseCases.Admin.Accounts;

namespace SafeDose.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/admins")]
[Authorize(Roles = "SuperAdmin")]   // only SuperAdmin can manage admin accounts
public class AdminAccountsController : ControllerBase
{
    private readonly ListAdminsUseCase        _list;
    private readonly CreateAdminUseCase       _create;
    private readonly UpdateAdminUseCase       _update;
    private readonly DeleteAdminUseCase       _delete;
    private readonly ToggleAdminStatusUseCase _toggle;

    public AdminAccountsController(
        ListAdminsUseCase list,
        CreateAdminUseCase create,
        UpdateAdminUseCase update,
        DeleteAdminUseCase delete,
        ToggleAdminStatusUseCase toggle)
    {
        _list   = list;
        _create = create;
        _update = update;
        _delete = delete;
        _toggle = toggle;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _list.ExecuteAsync(page, pageSize));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminDto dto)
    {
        try
        {
            var id = await _create.ExecuteAsync(dto);
            return CreatedAtAction(nameof(List), null, new { id });
        }
        catch (ArgumentException ex)         { return BadRequest(new { code = "VALIDATION", message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { code = "EMAIL_TAKEN", message = ex.Message }); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAdminDto dto)
    {
        try
        {
            var ok = await _update.ExecuteAsync(id, dto);
            return ok ? NoContent() : NotFound(new { code = "ADMIN_NOT_FOUND" });
        }
        catch (ArgumentException ex)         { return BadRequest(new { code = "VALIDATION", message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { code = "EMAIL_TAKEN", message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var ok = await _delete.ExecuteAsync(id);
            return ok ? NoContent() : NotFound(new { code = "ADMIN_NOT_FOUND" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { code = "LAST_SUPERADMIN", message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetStatus(string id, [FromBody] ToggleAdminStatusDto dto)
    {
        var ok = await _toggle.ExecuteAsync(id, dto.IsActive);
        return ok ? NoContent() : NotFound(new { code = "ADMIN_NOT_FOUND" });
    }
}
