using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.UseCases.Admin.Auth;

namespace SafeDose.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly AdminLoginUseCase _login;
    public AdminAuthController(AdminLoginUseCase login) => _login = login;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto req)
    {
        var result = await _login.ExecuteAsync(req);
        if (result == null)
            return Unauthorized(new
            {
                code = "INVALID_CREDENTIALS",
                messageArabic = "بيانات الدخول غير صحيحة أو الحساب ليس له صلاحية مشرف"
            });
        return Ok(result);
    }
}
