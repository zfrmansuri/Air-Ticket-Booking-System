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
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // POST: api/Booking/BookTicket
        //[HttpPost("BookTicket")]
        //public async Task<IActionResult> BookTicket([FromBody] BookingDto bookingDto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (userId == null)
        //        return Unauthorized(new { Message = "User not authorized." });

        //    var booking = new Booking
        //    {
        //        UserId = userId,
        //        FlightId = bookingDto.FlightId,
        //        BookingDate = DateTime.Now,
        //        NumberOfSeats = bookingDto.NumberOfSeats,
        //        Status = "Confirmed"
        //    };

        //    try
        //    {
        //        var bookingId = await _bookingService.BookTicket(booking);
        //        return Ok(new { BookingId = bookingId, Message = "Booking completed successfully." });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { Message = "Booking failed. The specified flight could not be found.", Details = ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { Message = "Booking failed due to invalid operation.", Details = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = "An error occurred while booking the ticket.", Details = ex.Message });
        //    }
        //}

        // GET: api/Booking/GetBookingHistory_Of_LoggedUser


        [HttpPost("BookTicket")]
        public async Task<IActionResult> BookTicket([FromBody] BookingDto bookingDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Retrieve the authenticated user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "User not authorized." });

            try
            {
                // Create a new booking entity
                var booking = new Booking
                {
                    UserId = userId,
                    FlightId = bookingDto.FlightId,
                    BookingDate = DateTime.Now,
                    Status = "Confirmed"
                };

                // Pass the booking entity and the list of SeatIds to the service
                var bookingId = await _bookingService.BookTicket(booking, bookingDto.SeatIds);

                return Ok(new { BookingId = bookingId, Message = "Booking completed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = "Booking failed. The specified flight or seat(s) could not be found.", Details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = "Booking failed due to invalid operation.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while booking the ticket.", Details = ex.Message });
            }
        }



        [HttpGet("GetBookingHistory_Of_LoggedUser")]
        public async Task<IActionResult> GetBookingHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "User not authorized." });

                var bookings = await _bookingService.GetBookingHistory(userId);

                if (bookings == null || !bookings.Any())
                    return NotFound(new { Message = "No booking history found for the current user." });

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving booking history.", Details = ex.Message });
            }
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
                return NotFound(new { Message = "Cancellation failed. The specified booking could not be found.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while canceling the booking.", Details = ex.Message });
            }
        }

        // GET: api/Booking/ListAllBooking
        [HttpGet("ListAllBooking")]
        [Authorize(Roles = "FlightOwner, Admin")]
        public async Task<IActionResult> ListAllBooking()
        {
            try
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = "Access denied. You do not have the necessary permissions.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving bookings.", Details = ex.Message });
            }
        }
    }
}
