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

        [HttpPost("AddFlight")]
        [Authorize(Roles = "FlightOwner")]  // Only FlightOwner can add flights
        public async Task<IActionResult> AddFlight([FromBody] FlightDto flightDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var flight = new Flight
            {
                //FlightId = 0,     //not assigning FlightId as its auto incremental property
                FlightNumber = flightDto.FlightNumber,
                Origin = flightDto.Origin,
                Destination = flightDto.Destination,
                DepartureDate = flightDto.DepartureDate,
                AvailableSeats = flightDto.AvailableSeats,
                PricePerSeat = flightDto.PricePerSeat,

                //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;    //

                FlightOwnerId = User.FindFirst(ClaimTypes.NameIdentifier).Value // Gets FlightOwnerId from the user's token //there can be a question mark to handle null values like above
            };

            await _flightService.AddFlight(flight);
            return Ok("Flight added successfully.");
        }

        [HttpPut("UpdateFlight/{id}")]
        [Authorize(Roles = "Admin,FlightOwner")]  // Only Admin or FlightOwner can update flights
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] FlightDto flightDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _flightService.UpdateFlight(id, flightDto , userId);
                return Ok("Flight updated successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "You are not authorized to update this flight." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("RemoveFlight/{id}")]
        [Authorize(Roles = "Admin,FlightOwner")]  // Only Admin or FlightOwner can remove flights
        public async Task<IActionResult> RemoveFlight(int id)
        {
            try
            {
                await _flightService.RemoveFlight(id);
                return Ok("Flight removed successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("SearchFlights")]
        [AllowAnonymous]  // Publicly accessible
        public async Task<IActionResult> SearchFlights([FromQuery] string origin, [FromQuery] string destination, [FromQuery] DateTime? date)
        {
            var flights = await _flightService.SearchFlights(origin, destination, date);
            return Ok(flights);
        }

        [HttpGet("GetFlightDetails/{id}")]
        [AllowAnonymous]  // Publicly accessible
        public async Task<IActionResult> GetFlightDetails(int id)
        {
            try
            {
                var flight = await _flightService.GetFlightDetails(id);
                return Ok(flight);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
