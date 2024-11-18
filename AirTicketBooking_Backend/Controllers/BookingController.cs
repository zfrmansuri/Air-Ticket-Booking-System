using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
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
        
        
        //Tried Manual AUuthorizarion
        [HttpPost("BookTicket")]
        public async Task<IActionResult> BookTicket([FromBody] BookingDto bookingDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User not authorized.");

            var booking = new Booking
            {
                UserId = userId,
                FlightId = bookingDto.FlightId,
                BookingDate = DateTime.Now,
                NumberOfSeats = bookingDto.NumberOfSeats,
                //TotalPrice  - This Field will be added by Services
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


        // GET: api/Booking/GetBookingHistory_Of_LoggedUser
        [HttpGet("GetBookingHistory_Of_LoggedUser")]
        public async Task<IActionResult> GetBookingHistory()     
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _bookingService.GetBookingHistory(userId);
            return Ok(bookings);
        }

        
        // DELETE: api/Booking/CancelBooking/{id}
        [HttpDelete("CancelBooking")]
        public async Task<IActionResult> CancelBooking(int booking_Id)
        {
            try
            {
                await _bookingService.CancelBooking(booking_Id);
                return Ok(new { Message = "Booking canceled successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpGet("ListAllBooking")]
        [Authorize(Roles = "FlightOwner, Admin")] 
        public async Task<IActionResult> ListAllBooking()     //List all the bookings if logged as ADMIN , while logged as FlightOwner display bookings for his flight only
        {
            // Get the current user id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Invalid access token." });

            // Check if the user is an Admin
            var isAdmin = User.IsInRole("Admin");

            // Get bookings based on the user's role
            IEnumerable<BookingRetrievalDto> bookings;

            if (isAdmin)
            {
                // Admin can see all bookings
                bookings = await _bookingService.ListAllBookingsForAdmin();
            }
            else
            {
                // FlightOwner can see only their bookings
                bookings = await _bookingService.ListAllBooking(userId);
            }

            // If no bookings found, return NotFound
            if (bookings == null || !bookings.Any())
                return NotFound(new { Message = "No bookings found." });

            return Ok(bookings);
        }


    }
}
