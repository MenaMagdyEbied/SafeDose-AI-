using SafeDose.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.Auth.ServicesInterfaces
{
    public interface IAuthService
    {
        Task<AuthModelDTO> RegisterAdminAsync(RegisterDTO model);
        Task<AuthModelDTO> RegisterAsync(RegisterDTO model);
        Task<string> ConfrimEmail(EmailConfirmationDTO EmailConfirmationModel);
        Task<string> ReSend(string Email);
        Task<AuthModelDTO> GetTokenAsync(LoginDTO model);
        Task<string> ForgotPass(ForgotPasswordDto ForgotPasswordModel);
        Task<string> ResetPass(ResetPasswordDto ResetPasswordModel);


    }
}
