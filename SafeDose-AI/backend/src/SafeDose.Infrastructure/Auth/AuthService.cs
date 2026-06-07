using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SafeDose.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Account> _userManager;
        private readonly JWT _jwt;
        public AuthService(UserManager<Account> userManager, IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
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
                return new AuthModelDTO { Message = "Account created successfuly" };
            }

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
    }
}
