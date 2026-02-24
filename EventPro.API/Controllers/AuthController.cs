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
using EventPro.DAL.Common;

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
                var user = await AuthenticateUser(loginModel.Username, loginModel.Password);

                if (user == null)
                    return Unauthorized("Invalid username or password");

                var validationError = ValidateUserAccount(user);
                if (validationError != null)
                    return BadRequest(validationError);

                var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == user.Role);
                if (role == null)
                    return BadRequest("User role not found");

                var token = GenerateJwtToken(user, role);

                await UpdateDeviceIdIfChanged(user, loginModel.DeviceId);

                return Ok(new
                {
                    user.FirstName,
                    user.LastName,
                    role.RoleName,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            catch (Exception ex)
            {
                // Log exception here if logging is configured
                return StatusCode(500, $"An error occurred during login {ex}");
            }
        }

        private async Task<Users> AuthenticateUser(string username, string password)
        {
            var allowedRoles = await db.Roles
                .Where(r => r.Id == RoleIds.GateKeeper || r.Id == RoleIds.Client )
                .Select(r => r.Id)
                .ToListAsync();

            return await db.Users
                .FirstOrDefaultAsync(u =>
                    u.UserName == username &&
                    u.Password == password &&
                    allowedRoles.Contains(u.RoleNavigation.Id));
        }

        private string ValidateUserAccount(Users user)
        {
            if (user.IsActive == false)
                return "Account locked, please contact system administrator";

            if (user.Approved == false)
                return "Account Not Approved, please contact system administrator";

            return null;
        }

        private JwtSecurityToken GenerateJwtToken(Users user, Roles role)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Sid, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, role.RoleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            return new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(2),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
        }

        private async Task UpdateDeviceIdIfChanged(Users user, string newDeviceId)
        {
            if (user.DeviceId != newDeviceId)
            {
                user.DeviceId = newDeviceId;
                await db.SaveChangesAsync();
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
