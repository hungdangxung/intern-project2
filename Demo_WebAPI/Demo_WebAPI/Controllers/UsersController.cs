using Demo_WebAPI.Data;
using Demo_WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Demo_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly AppSettings _appSettings;

        public UsersController(MyDbContext context, IOptionsMonitor<AppSettings> optionsMonitor) 
        {
            _context = context;
            _appSettings = optionsMonitor.CurrentValue;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
            if (user == null)
            {
                var user1 = new User
                {
                    UserName = model.UserName,
                    Password = model.Password,
                    FullName = model.FullName,
                    Email = model.Email
                };
                _context.Users.Add(user1);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Success = true,
                    Data = user1
                });
            }
            return BadRequest("Username already exists");
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == model.UserName && 
            u.Password == model.Password);
            if (user == null)
            {
                return Ok(new
                {
                    Success = false,
                    Message = "Invalid user name/password"
                });
            }
            var token = await GenerateToken(user);
            return Ok(new
            {
                Success = true,
                Message = "Authenticate Success",
                Data = token
            });
        }
        private async Task<String> GenerateToken(User user)
        {
            var jwtTokenHanlder = new JwtSecurityTokenHandler();
            var securityKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim("UserName", user.UserName),
                    new Claim("Id", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKeyBytes),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jwtTokenHanlder.CreateToken(tokenDescription);
            var accessToken = jwtTokenHanlder.WriteToken(token);
            return accessToken;
        }
    }
}
