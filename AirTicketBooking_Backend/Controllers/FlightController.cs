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
        [HttpPost("AddFlight")]              // Only FlightOwner can add flights we can also add funtionality to allow 
        [Authorize(Roles = "FlightOwner")]           //Admin to add file too but then we will have to pass the FlightOwner Id too in the Post request
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
                    FlightOwnerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value            // Gets FlightOwnerId from the user's token //there can be a question mark to handle null values like above
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
        [Authorize(Roles = "Admin,FlightOwner")]            //Only Admin can Update all Fight & Flight Owner can Update his Flight only.
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
        [Authorize(Roles = "Admin,FlightOwner")]             // Only Admin and FlightOwner can remove flights  
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
        [AllowAnonymous]            // Publicly accessible
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
        [AllowAnonymous]            // Publicly accessible
        public async Task<IActionResult> GetFlightDetails(int id)            //GetFlightDetailsById
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
    }
}
