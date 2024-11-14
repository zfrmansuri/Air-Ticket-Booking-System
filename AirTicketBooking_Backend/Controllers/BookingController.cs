using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirTicketBooking_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        
        
        // POST: api/Booking/BookTicket
        [HttpPost("BookTicket")]
        public async Task<IActionResult> BookTicket([FromBody] BookingDto bookingDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);   //put a breakpoint and check whats comming here
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User not authorized.");

            var booking = new Booking
            {
                UserId = userId,
                FlightId = bookingDto.FlightId,
                BookingDate = DateTime.Now,
                NumberOfSeats = bookingDto.NumberOfSeats,
                //TotalPrice = (flight.PricePerSeat * bookingDto.NumberOfSeats),
                Status = "Confirmed"
            };

            try
            {
                var bookingId = await _bookingService.BookTicket(booking);
                return Ok(new { BookingId = bookingId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        

        // GET: api/Booking/GetBookingHistory
        [HttpGet("GetBookingHistory")]
        public async Task<IActionResult> GetBookingHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _bookingService.GetBookingHistory(userId);
            return Ok(bookings);
        }

        /*
        // DELETE: api/Booking/CancelBooking/{id}
        [HttpDelete("CancelBooking/{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                await _bookingService.CancelBooking(id);
                return Ok(new { Message = "Booking canceled successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }*/
    }
}
