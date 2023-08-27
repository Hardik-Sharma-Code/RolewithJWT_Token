using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoleBase.DataBase.Model;
using RoleBase.DataBase.Tables;
using System.Net;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAPI.RoleBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost]
        [Route("Registor")]
        public async Task<IActionResult> Create([FromBody] Registor registor)
        {
            if (ModelState.IsValid)
            {
                var userCheck = await userManager.FindByEmailAsync(registor.Email);
                if(userCheck != null)
                {
                    return StatusCode(StatusCodes.Status502BadGateway, registor.Email + " is already created");
                }
                ApplicationUser _user = new ApplicationUser
                {
                    FirstName = registor.FirstName,
                    LastName = registor.LastName,
                    Email = registor.Email,
                    PasswordHash = registor.Password,
                };
                IdentityResult result = await userManager.CreateAsync(_user);
                if (result.Succeeded)
                {
                    return Ok("Created");
                }
                else
                {
                    return StatusCode(StatusCodes.Status502BadGateway, "Something went wrong");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status502BadGateway, "Something went wrong");
            }
            
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] User model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status200OK, "User not found");
                }

                if (await userManager.CheckPasswordAsync(user, model.Password) == false)
                {
                    return StatusCode(StatusCodes.Status200OK, "Invalid credentials");

                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,false,false);

                if (result.Succeeded)
                {
                    await userManager.AddClaimAsync(user, new Claim("UserRole", "Admin"));
                    return StatusCode(StatusCodes.Status200OK, "success");
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, "Invalid login");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status502BadGateway, "Please check username or password");
            }
        }

        [HttpPost]
        [Route("LoginOut")]
        public async Task<IActionResult> Loginout()
        {
            await signInManager.SignOutAsync();
            return StatusCode(StatusCodes.Status200OK);
        }

    }
}
