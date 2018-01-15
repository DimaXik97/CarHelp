using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarHelp.WebApi.DbContext;
using CarHelp.WebApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarHelp.WebApi.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
             _userManager = userManager;
            _signInManager = signInManager;
        }


        [Route("login")]
        [HttpPost]
        public async Task<string> PostAsync([FromBody]LoginDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user==null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return "Неправильный логин и (или) пароль";
                }
                else{
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                    return "OK";
                }
            }
            else
                return "Error model";
        }

        [Route("register")]
        [HttpPost]
        public async Task<string> PostAsync([FromBody]RegisterDTO model)
        {
            ApplicationUser user = new ApplicationUser { Email = model.Email, UserName = model.Email};
                // добавляем пользователя
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // установка куки
                await _signInManager.SignInAsync(user, false);
                return "OK";
            }
            else{
                return result.Errors.FirstOrDefault().Description;
            }
        }

        [Route("lodout")]
        [HttpPost]
        public async Task<string> PostAsync()
        {
            await _signInManager.SignOutAsync();
            return "successful";
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
