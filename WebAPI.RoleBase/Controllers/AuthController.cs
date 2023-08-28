using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoleBase.DataBase.Model;
using RoleBase.DataBase.Tables;
using RoleBase.Services.Services.Interface;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAPI.RoleBase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuthController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public AuthController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserVM model)
        {
            var result = await _userServices.LoginWithToken(model);
            return StatusCode(StatusCodes.Status200OK, "token: " + result.Token);
        }

        [HttpPost]
        [Route("Registor")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] RegistorVM registor)
        {
            if (ModelState.IsValid)
            {
                var newRegistor = await _userServices.Registor(registor);
                return StatusCode(StatusCodes.Status200OK, newRegistor.Message);
            }
            return StatusCode(StatusCodes.Status502BadGateway, "Something went wrong");

        }

        [HttpGet]
        [Route("LogOut")]
        public async Task<IActionResult> Logout()
        {
            await _userServices.LogOut();
            return StatusCode(StatusCodes.Status200OK);
        }

        ///for Roles
        [HttpGet]
        [Route("GetRoles")]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _userServices.GetAllRole();
            return StatusCode(StatusCodes.Status200OK, roles);
        }

        [HttpGet]
        [Route("GetRolesById")]
        public async Task<IActionResult> GetById([FromRoute] string name)
        {
            var roles = await _userServices.GetRoleByName(name);
            return StatusCode(StatusCodes.Status200OK, roles);
        }

        [HttpPost]
        [Route("addRole")]
        public async Task<IActionResult> Role([FromBody] RolesVM model)
        {
            if (ModelState.IsValid)
            {
                var addRole = await _userServices.addRole(model);
                return StatusCode(StatusCodes.Status502BadGateway, addRole.Message);
            }
            else
            {
                return StatusCode(StatusCodes.Status502BadGateway, "Something went wrong");
            }
        }
    }
}
