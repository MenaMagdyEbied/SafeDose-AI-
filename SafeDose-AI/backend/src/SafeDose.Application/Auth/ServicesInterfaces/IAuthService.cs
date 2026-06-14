using SafeDose.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.Auth.ServicesInterfaces
{
    public interface IAuthService
    {
        Task<AuthModelDTO> RegisterAsync(RegisterDTO model);
        Task<string> ConfrimEmail(EmailConfirmationDTO EmailConfirmationModel);
        Task<AuthModelDTO> GetTokenAsync(LoginDTO model);
        Task<string> ForgotPass(ForgotPasswordDto ForgotPasswordModel);
        Task<string> ResetPass(ResetPasswordDto ResetPasswordModel);


    }
}
