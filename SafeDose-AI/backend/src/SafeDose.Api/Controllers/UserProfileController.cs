using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.UserProfile.DTOs;
using SafeDose.Application.UserProfile.ServicesInterface;
using SafeDose.Infrastructure.UserProfile.ServicesImplementation;

namespace SafeDose.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileServices _userProfileServices;
        public UserProfileController(IUserProfileServices userProfileServices)
        {
            _userProfileServices = userProfileServices; 
        }

        [HttpGet("userProfile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                UserGetProfileDTO userGetProfileDTO = await _userProfileServices.GetUserProfile();
                return Ok(userGetProfileDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


        [HttpPut("updateName")]
        public async Task<IActionResult> UpdateName(UserUpdateNameDTO dto)
        {
            try
            {
                string  result = await _userProfileServices.UpdateName(dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("updateEmail")]
        public async Task<IActionResult> UpdateEmail(UserUpdateEmailDTO dto)
        {
            try
            {
                string result = await _userProfileServices.UpdateEmail(dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("updatePhone")]
        public async Task<IActionResult> UpdatePhone(UserUpdatePhoneDTO dto)
        {
            try
            {
                string result = await _userProfileServices.UpdatePhone(dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
