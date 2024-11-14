//using AirTicketBooking_Backend.DTOs;
//using AirTicketBooking_Backend.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Text;

//namespace AirTicketBooking_Backend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AccountController : ControllerBase
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly IConfiguration _configuration;

//        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
//        {
//            _userManager = userManager;
//            _configuration = configuration;
//        }

//        [HttpPost("Register")]
//        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
//        {
//            if (model.Password != model.ConfirmPassword)
//            {
//                return BadRequest("Passwords do not match");
//            }

//            var user = new ApplicationUser
//            {
//                UserName = model.Email,
//                Email = model.Email,
//                FullName = model.FullName
//            };

//            var result = await _userManager.CreateAsync(user, model.Password);

//            if (!result.Succeeded)
//            {
//                return BadRequest(result.Errors);
//            }

//            return Ok("User registered successfully");
//        }

//        [HttpPost("Login")]
//        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
//        {
//            var user = await _userManager.FindByEmailAsync(model.Email);
//            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
//            {
//                return Unauthorized("Invalid email or password");
//            }

//            var token = GenerateJwtToken(user);
//            return Ok(new AuthResponseDto { Token = token, Expiration = DateTime.UtcNow.AddHours(1) });
//        }

//        private string GenerateJwtToken(ApplicationUser user)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new System.Security.Claims.ClaimsIdentity(new[]
//                {
//                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.Email),
//                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//                }),
//                Expires = DateTime.UtcNow.AddHours(1),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            return tokenHandler.WriteToken(token);
//        }
//    }
//}
