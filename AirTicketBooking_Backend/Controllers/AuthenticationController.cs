using AirTicketBooking_Backend.Authentication;
using AirTicketBooking_Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using AirTicketBooking_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AirTicketBooking_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUsersAuthenticationService _authService;

        public AuthenticationController(IUsersAuthenticationService authService)
        {
            _authService = authService;
        }

        // POST: api/Authentication/RegisterUser
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                Gender = model.Gender,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _authService.RegisterUser(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User registered successfully!");
        }

        // POST: api/Authentication/RegisterFlightOwner
        [Authorize(Roles = "Admin")]
        [HttpPost("RegisterFlightOwner")]
        public async Task<IActionResult> RegisterFlightOwner([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                Gender = model.Gender,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _authService.RegisterFlightOwner(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Flight Owner registered successfully!");
        }

        // POST: api/Authentication/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _authService.Login(model.Email, model.Password);
                return Ok(new { Token = token });                                  // why we are coying token to other variable why we will see later
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
