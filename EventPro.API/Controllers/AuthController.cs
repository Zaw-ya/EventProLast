using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EventPro.API.Models;
using EventPro.DAL.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventPro.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                var gateRoleId = (await db.Roles.Where(p => p.RoleName == "Gatekeeper").FirstOrDefaultAsync())?.Id;
                var clintRoleId = (await db.Roles.Where(p => p.RoleName == "Client").FirstOrDefaultAsync())?.Id;

                var user = await db.Users.Where(p => p.UserName == loginModel.Username
                && p.Password == loginModel.Password && (p.Role == gateRoleId || p.Role == clintRoleId)).FirstOrDefaultAsync();

                if (user != null)
                {
                    if (!Convert.ToBoolean(user.IsActive))
                        return BadRequest("Account locked, please contact system administrator");

                    if (!Convert.ToBoolean(user.Approved))
                        return BadRequest("Account Not Approved, please contact system administrator");

                    var role = await db.Roles.Where(p => p.Id == user.Role).FirstOrDefaultAsync();

                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Sid, Convert.ToString(user.UserId)),
                    new Claim(ClaimTypes.Role, role.RoleName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddDays(2),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    if (user.DeviceId != loginModel.DeviceId)
                    {
                        user.DeviceId = loginModel.DeviceId;
                        await db.SaveChangesAsync();
                    }
                    return Ok(new
                    {
                        user.FirstName,
                        user.LastName,
                        role.RoleName,
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }

                else
                    return Unauthorized();
            }
            catch (Exception EX)
            {

                return Unauthorized();
            }
        }

        [HttpPost(template: "Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            registerModel.UserName = registerModel.UserName.Trim();
            var user = await db.Users.Where(p => p.UserName == registerModel.UserName).FirstOrDefaultAsync();

            if (user != null)
                return BadRequest("This GateKeeper/Client username already exists.");
            var roleTypeId = (await db.Roles.Where(r => r.RoleName.ToLower() == registerModel.Role.ToLower())
                             .FirstOrDefaultAsync())?.Id;

            var newUser = new Users
            {
                UserName = registerModel.UserName,
                Password = registerModel.Password,
                IsActive = true,
                Approved = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = 1,
                Email = registerModel.Email,
                CityId = registerModel.CityId,
                Role = roleTypeId,
                Gender = registerModel.Gender,
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                PrimaryContactNo = registerModel.PhoneNumber
            };

            await db.Users.AddAsync(newUser);
            await db.SaveChangesAsync();

            return Ok("You have been successfully registered!. Please wait for the admin approval");
        }
    }
}
