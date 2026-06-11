using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.UseCases;
using SafeDose.Domain.Entities;

namespace SafeDose.Api.Controllers;


[ApiController]
[Route("api/interactions")]
public class InteractionsController : ControllerBase
{
    //private readonly CheckDrugInteractionUseCase _useCase;

    //public InteractionsController(CheckDrugInteractionUseCase useCase)
    //{
    //    _useCase = useCase;
    //}

    private readonly IUserGlobalServices _userGlobalServices;
    public InteractionsController(IUserGlobalServices userGlobalServices)
    {
        _userGlobalServices = userGlobalServices;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        Account account = await _userGlobalServices.GetUser(); 
        return Ok("ggggg" + account.UserName);
    }




    //[HttpPost("check")]
    //public async Task<IActionResult> Check([FromBody] CheckInteractionRequest request)
    //{
    //    var result = await _useCase.ExecuteAsync(request.PatientId, request.Drugs);
    //    return Ok(result);
    //}
}

//public record CheckInteractionRequest(int PatientId, string[] Drugs);
