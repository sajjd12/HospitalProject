using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hospital.Core.DTOs;

namespace Hospital.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if(user != null && await _userManager.CheckPasswordAsync(user,model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var AuthClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("FullName", user.FullName),
                    new Claim("UserId", user.Id)
                };
                foreach (var UserRole in userRoles)
                {
                    AuthClaims.Add(new Claim(ClaimTypes.Role, UserRole));
                }
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:DurationInDays"])),
                    claims: AuthClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    username = user.UserName
                });
            }
            return Unauthorized("اسم المستخدم أو كلمة المرور غير صحيحة");
        }
    }
}
