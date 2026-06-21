using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.ReminderResponse.DTOs;
using SafeDose.Application.ReminderResponse.ServicesInterface;

namespace SafeDose.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderResponseController : ControllerBase
    {
        private readonly IReminderResponseServices _reminderResponseServices;
        public ReminderResponseController(IReminderResponseServices reminderResponseServices)
        {
            _reminderResponseServices = reminderResponseServices;       
        }

        [HttpPost("addReminderResponse")]
        public async Task<IActionResult> AddReminderResponse(ReminderResponseAddDTO dto)
        {
            try
            {
                string result = await _reminderResponseServices.Add(dto);
                return Ok(new { message = result });
            }
            catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            } 
        }


        [HttpGet("{PatientId}")]
        public async Task<IActionResult> GetReminderResponse(int PatientId)
        {
            try
            {
                List<ReminderResponseGetDTO> reminderResponseGetDTO = await _reminderResponseServices.GetReminderResponse(PatientId); 
                return Ok(reminderResponseGetDTO);      
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }
    }
}
