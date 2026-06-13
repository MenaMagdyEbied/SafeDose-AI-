using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SafeDose.Application.Auth.DTOs;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Domain.Entities;
using SafeDose.Shared.helper;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SafeDose.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<Account> _userManager;
        private readonly JWT _jwt;
        private readonly IEmailSender _emailSender;
        public AuthService(UserManager<Account> userManager, IOptions<JWT> jwt, IEmailSender emailSender)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _emailSender = emailSender;
        }

        public async Task<AuthModelDTO> RegisterAsync(RegisterDTO model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModelDTO { Message = "Email is already registerd!" };

            if (await _userManager.FindByNameAsync(model.UserName) is not null)
                return new AuthModelDTO { Message = "UserName is already registerd!" };

            
            List<Account>? accounts = await _userManager.Users.Where(u => u.PhoneNumber == model.PhoneNumber).ToListAsync();
            if(accounts.Count() > 0)
                return new AuthModelDTO { Message = "PhoneNumber is already registerd!" };

            Account account = new Account();    
            account.Name = model.FullName;
            account.Email = model.Email;
            account.UserName = model.UserName;
            account.PhoneNumber = model.PhoneNumber;

            IdentityResult result = await _userManager.CreateAsync(account, model.Password);

            if (!result.Succeeded)
            {
                string errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += error.Description + " , ";
                }

                return new AuthModelDTO { Message = errors };
            }
            else
            {
                await _userManager.AddToRoleAsync(account, "User");

                // generate token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(account);
                var encodedToken = HttpUtility.UrlEncode(token);

                // send Email . 

                await _emailSender.SendEmailAsync(account.Email, "Email Confirmation",
                $"<p>Code : {encodedToken} </p>");

                return new AuthModelDTO { Message = "Account created successfuly and waiting to Confirmation by email" };
            }

        }

        public async Task<string> ConfrimEmail(EmailConfirmationDTO EmailConfirmationModel)
        {
            if (EmailConfirmationModel.Email == null || EmailConfirmationModel.Code == null)
                return "email or code equal null";

            Account? account = await _userManager.FindByEmailAsync(EmailConfirmationModel.Email);
            if (account == null)
                return "Email is not registerd!";

            if(await _userManager.IsEmailConfirmedAsync(account) == true)
                return "Email is already confirmed!";

            var decodedToken = HttpUtility.UrlDecode(EmailConfirmationModel.Code);

            var result = await _userManager.ConfirmEmailAsync(account, decodedToken);
            if (result.Succeeded)
                return "Email Confirmed";


            return "something went wrong";
        }

        public async Task<AuthModelDTO> GetTokenAsync(LoginDTO model)
        {
            AuthModelDTO authModel = new AuthModelDTO();

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "userName or password is wrong!";
                return authModel;
            }

            if (await _userManager.IsEmailConfirmedAsync(user) == false)
            {
                authModel.Message = "Email is not confirmed please confirm your email address";
                return authModel;   
            }

            var jwtSecurityToken = await CreateJwtToken(user);

            authModel.Message = "Token created successfully!";
            authModel.UserName = user.UserName;
            authModel.Email = user.Email;
            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;

            return authModel;

        }





        private async Task<JwtSecurityToken> CreateJwtToken(Account user)
        {

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>();

            // Standard claims
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            // Role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

      

        public async Task<string> ForgotPass(ForgotPasswordDto ForgotPasswordModel)
        {
            var user = await _userManager.FindByEmailAsync(ForgotPasswordModel.Email);
            if (user == null)
                throw new Exception("Invalid Email");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            //var resetLink = $"{ForgotPasswordModel.ClientUrl}?email={user.Email}&token={encodedToken}";

            //await _emailSender.SendEmailAsync(user.Email, "Reset Password",
            //    $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>");

            await _emailSender.SendEmailAsync(user.Email, "Reset Password",
                $"<p>Code : {encodedToken}</p>");

            return "Password reset email sent.";
        }

        public async Task<string> ResetPass(ResetPasswordDto ResetPasswordModel)
        {
            var user = await _userManager.FindByEmailAsync(ResetPasswordModel.Email);
            if (user == null)
                throw new Exception("Invalid Email");

            var decodedToken = HttpUtility.UrlDecode(ResetPasswordModel.Code);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, ResetPasswordModel.NewPassword);
            if (!result.Succeeded)
                throw new Exception(result.Errors.ToString());

            return "Password reset successful.";
        }



    }
}
