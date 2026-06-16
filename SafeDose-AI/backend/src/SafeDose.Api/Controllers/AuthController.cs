using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.Auth.DTOs;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Infrastructure.Auth;

namespace SafeDose.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            try 
            {
                AuthModelDTO result = await _authService.RegisterAsync(dto);
                return Ok(result.Message);  
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }



        [HttpPost("emailConfirmation")]
        public async Task<IActionResult> EmailConfirmation(EmailConfirmationDTO dto)
        {
            try
            {
                string result = await _authService.ConfrimEmail(dto);
                return Ok(result);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);      
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            try
            {
                AuthModelDTO result = await _authService.GetTokenAsync(dto);

                if (!result.IsAuthenticated)
                    return BadRequest(result.Message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            try
            {
                string result = await _authService.ForgotPass(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            try
            {
                string result = await _authService.ResetPass(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
