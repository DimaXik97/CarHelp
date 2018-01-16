using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CarHelp.WebApi.DbContext;
using CarHelp.WebApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CarHelp.WebApi.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IConfiguration _configuration;
        public AccountController(IConfiguration configuration, UserManager<ApplicationUser> userManager, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _configuration=configuration;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
        }


        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]LoginDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
 
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
            {
                return BadRequest();
            }
            var token = await GetJwtSecurityToken(user);
 
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
              
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]RegisterDTO model)
        {
            ApplicationUser user = new ApplicationUser { Email = model.Email, UserName = model.Email};
                // добавляем пользователя
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // установка куки
                return Ok();
            }
            else{
                return BadRequest(result.Errors.FirstOrDefault().Description);
            }
        }

        private async Task<JwtSecurityToken> GetJwtSecurityToken(ApplicationUser user)
        {
            var userClaims =  await _userManager.GetClaimsAsync(user);
            Debug.WriteLine("dfdf" + userClaims);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            return new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtIssuer"],
                notBefore: DateTime.UtcNow,
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtExpireMin"])),
                signingCredentials: creds
            );
        }
    }
    public class LoginDTO{
        public string Email{get;set;}
        public string Password{get;set;}
    }
    public class RegisterDTO{
        public string Email{get;set;}
        public string Password{get;set;}
        public string ConfirmPassword{get;set;}
    }    
}
