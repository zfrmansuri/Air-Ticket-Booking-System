using AirTicketBooking_Backend.Authentication;
using AirTicketBooking_Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using AirTicketBooking_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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

        [Authorize]
        [HttpPut("EditProfile/{id}")]
        public async Task<IActionResult> EditProfile(string id, [FromBody] EditProfileDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Only Admin can edit anyone's profile, others can edit their own
                if (User.IsInRole("Admin") || userId == id.ToString())
                {
                    await _authService.EditProfile(id, model, userId);
                    return Ok("Profile updated successfully.");
                }
                else
                {
                    return Unauthorized(new { message = "You do not have permission to edit this profile." });
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }


        [Authorize]
        [HttpDelete("DeleteProfile/{id}")]
        public async Task<IActionResult> DeleteProfile(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Only Admin can delete anyone's profile, others can delete their own
                if (User.IsInRole("Admin") || userId == id.ToString())
                {
                    await _authService.DeleteProfile(id, userId);
                    return Ok("Profile deleted successfully.");
                }
                else
                {
                    return Unauthorized(new { message = "You do not have permission to delete this profile." });
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }



        [HttpGet("GetUsersByRole/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var users = await _authService.GetAllUsersByRole(role);

                if (users == null || !users.Any())
                    return NotFound(new { Message = $"No users found with role: {role}" });

                return Ok(users.Select(user => new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.Gender,
                    user.Address
                }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


    }
}
