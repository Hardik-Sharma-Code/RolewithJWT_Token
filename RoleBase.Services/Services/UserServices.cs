using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RoleBase.DataBase.JWT;
using RoleBase.DataBase.Model;
using RoleBase.DataBase.Tables;
using RoleBase.Services.Services.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RoleBase.Services.Services
{
    public class UserService : IUserServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
        }

        public async Task<HttpMessageResponse> addRole(RolesVM model)
        {
            var response = new HttpMessageResponse();
            try
            {
                var _role = await _roleManager.FindByNameAsync(model.Name);

                if (_role != null)
                {
                    response.Message = "Role is already added";
                    return response;
                }

                IdentityRole role = new IdentityRole
                {
                    Name = model.Name,
                };

                IdentityResult result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                     response.Message = "success";
                }
                    return response;

            }
            catch (Exception ex)
            {
                response.Message = "success";
                return response;
            }
        }

        public async Task<IList<IdentityRole>> GetAllRole()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles;
        }

        public async Task<IdentityRole> GetRoleByName(string name)
        {
            var roleByName = await _roleManager.FindByNameAsync(name);
            return roleByName;
        }

        public async Task<HttpMessageResponse> LoginWithToken(UserVM model)
        {
            var response = new HttpMessageResponse();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                response.Message = $"No Accounts Registered with {model.Email}.";
                return response;
            }
            if (await _userManager.CheckPasswordAsync(user, model.Password) == false)
            {

                response.Message = $"Incorrect Credentials for user {user.Email}.";
            }
            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
                response.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                return response;
            }
            response.Message = $"Incorrect Credentials for user {user.Email}.";
            return response;
        }

        public async Task LogOut()
        {
           await _signInManager.SignOutAsync();

        }

        public async Task<HttpMessageResponse> Registor(RegistorVM model)
        {
            HttpMessageResponse response = new HttpMessageResponse();
            try
            {
                var userCheck = await _userManager.FindByEmailAsync(model.Email);
                if (userCheck != null)
                {
                    response.Message = model.Email + " is already created";
                    return response;
                }
                ApplicationUser _user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.FirstName + model.LastName,
                };

                IdentityResult result = await _userManager.CreateAsync(_user);
                if (result.Succeeded)
                {
                    try
                    {
                        await _userManager.AddPasswordAsync(_user, model.Password);
                        await _userManager.AddToRoleAsync(_user, model.Role);
                        response.Message = "created";
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                return response;
            }

        }


        /// <summary>
        /// private method for creating JwtToken
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }
    }
}
