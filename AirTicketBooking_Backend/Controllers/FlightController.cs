using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AirTicketBooking_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        // POST: api/Flight/AddFlight
        [HttpPost("AddFlight")]
        [Authorize(Roles = "FlightOwner")]
        public async Task<IActionResult> AddFlight([FromBody] FlightDto flightDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var flight = new Flight
                {
                    FlightNumber = flightDto.FlightNumber,
                    Origin = flightDto.Origin,
                    Destination = flightDto.Destination,
                    DepartureDate = flightDto.DepartureDate,
                    AvailableSeats = flightDto.AvailableSeats,
                    PricePerSeat = flightDto.PricePerSeat,
                    FlightOwnerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                };

                await _flightService.AddFlight(flight);
                return Ok(new { Message = "Flight added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding the flight.", Details = ex.Message });
            }
        }

        // PUT: api/Flight/UpdateFlight/{id}
        [HttpPut("UpdateFlight/{id}")]
        [Authorize(Roles = "Admin,FlightOwner")]
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] FlightDto flightDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _flightService.UpdateFlight(id, flightDto, userId);
                return Ok(new { Message = "Flight updated successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { Message = "You are not authorized to update this flight." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = "The flight could not be found.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the flight.", Details = ex.Message });
            }
        }

        // DELETE: api/Flight/RemoveFlight/{id}
        [HttpDelete("RemoveFlight/{id}")]
        [Authorize(Roles = "Admin,FlightOwner")]
        public async Task<IActionResult> RemoveFlight(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                await _flightService.RemoveFlight(id, userId, isAdmin);
                return Ok(new { Message = "Flight removed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = "The flight could not be found.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while removing the flight.", Details = ex.Message });
            }
        }

        // GET: api/Flight/SearchFlights
        [HttpGet("SearchFlights")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchFlights([FromQuery] string origin, [FromQuery] string destination, [FromQuery] DateTime? date)
        {
            try
            {
                var flights = await _flightService.SearchFlights(origin, destination, date);

                if (flights == null || !flights.Any())
                    return NotFound(new { Message = "No flights found matching the search criteria." });

                return Ok(flights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while searching for flights.", Details = ex.Message });
            }
        }

        // GET: api/Flight/GetFlightDetails/{id}
        [HttpGet("GetFlightDetails/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFlightDetails(int id)
        {
            try
            {
                var flight = await _flightService.GetFlightDetails(id);

                if (flight == null)
                    return NotFound(new { Message = "The flight could not be found." });

                return Ok(flight);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = "The flight could not be found.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving flight details.", Details = ex.Message });
            }
        }

        [HttpGet("GetAllFlights")]
        [Authorize(Roles = "Admin,FlightOwner")] // Only Admin and FlightOwner can access
        public async Task<IActionResult> GetAllFlights()
        {
            try
            {
                // Get the current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Invalid access token." });

                // Check if the user is an Admin
                var isAdmin = User.IsInRole("Admin");

                IEnumerable<Flight> flights;

                if (isAdmin)
                {
                    // Admin can see all flights, so we pass null to GetAllFlights (no filtering by owner)
                    flights = await _flightService.GetAllFlights(null);
                }
                else
                {
                    // FlightOwner can only see their own flights
                    flights = await _flightService.GetAllFlights(userId);
                }

                // If no flights are found, return NotFound
                if (flights == null || !flights.Any())
                    return NotFound(new { Message = "No flights available." });

                return Ok(flights);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = "Access denied. You do not have the necessary permissions.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving flights.", Details = ex.Message });
            }
        }


        [HttpGet("GetAllFlightsForEveryone")]
        public async Task<IActionResult> GetAllFlightsForEveryone()
        {
            try
            {
                var flights = await _flightService.GetAllFlightsForEveryone();

                if (flights == null || !flights.Any())
                {
                    return NotFound(new { Message = "No flights are currently available." });
                }

                // Return only the necessary flight details
                var flightDetails = flights.Select(f => new
                {
                    f.FlightId,
                    f.FlightNumber,
                    f.Origin,
                    f.Destination,
                    f.DepartureDate,
                    f.AvailableSeats,
                    f.PricePerSeat
                }).ToList();

                return Ok(flightDetails); // Return HTTP 200 with the selected flight data
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching flights.", Details = ex.Message });
            }
        }

    }
}