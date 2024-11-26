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

            try
            {
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
                    return BadRequest(new { Message = "User registration failed.", Errors = result.Errors });

                return Ok(new { Message = "User registered successfully!" });
            }
            catch (Exception ex)
            {
                // Generic catch-all exception handler
                return StatusCode(500, new { Message = "An error occurred while registering the user.", Details = ex.Message });
            }
        }

        // POST: api/Authentication/RegisterFlightOwner
        [Authorize(Roles = "Admin")]
        [HttpPost("RegisterFlightOwner")]
        public async Task<IActionResult> RegisterFlightOwner([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
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
                    return BadRequest(new { Message = "Flight owner registration failed.", Errors = result.Errors });

                return Ok(new { Message = "Flight Owner registered successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while registering the flight owner.", Details = ex.Message });
            }
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
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = "Invalid login credentials.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while attempting to log in.", Details = ex.Message });
            }
        }

        // PUT: api/Authentication/EditProfile/{id}
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
                    return Ok(new { Message = "Profile updated successfully." });
                }
                else
                {
                    return Unauthorized(new { Message = "You do not have permission to edit this profile." });
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = "Profile not found.", Details = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = "Access denied.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the profile.", Details = ex.Message });
            }
        }

        // DELETE: api/Authentication/DeleteProfile/{id}
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
                    return Ok(new { Message = "Profile deleted successfully." });
                }
                else
                {
                    return Unauthorized(new { Message = "You do not have permission to delete this profile." });
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = "Profile not found.", Details = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = "Access denied.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the profile.", Details = ex.Message });
            }
        }

        // GET: api/Authentication/GetUsersByRole/{role}
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
                return BadRequest(new { Message = "Invalid role specified.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving users by role.", Details = ex.Message });
            }
        }
    }
}
