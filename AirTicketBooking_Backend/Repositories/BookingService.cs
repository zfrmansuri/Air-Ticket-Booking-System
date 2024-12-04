using AirTicketBooking_Backend.Data;
using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AirTicketBooking_Backend.Repositories
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<int> BookTicket(Booking booking)
        //{
        //    var flight = await _context.Flights.FindAsync(booking.FlightId);

        //    if (flight == null) throw new KeyNotFoundException("Flight not found.");
        //    if (booking.NumberOfSeats > flight.AvailableSeats) throw new InvalidOperationException("Insufficient seats available.");

        //    // Get all available seats for the specified flight
        //    var availableSeats = _context.FlightSeats
        //        .Where(fs => fs.FlightId == booking.FlightId && fs.IsAvailable)
        //        .ToList();


        //    if (availableSeats.Count < booking.NumberOfSeats)
        //        throw new InvalidOperationException($"Can't Meet No.of Seat requirement Only '{availableSeats.Count}' Seats are Availble");

        //    // Select the exact number of seats needed
        //    var seatsToReserve = availableSeats.Take(booking.NumberOfSeats).ToList();

        //    // Mark each selected seat as unavailable
        //    seatsToReserve.ForEach(fs => fs.IsAvailable = false);

        //    // update total price of booking
        //    booking.TotalPrice = flight.PricePerSeat * booking.NumberOfSeats;

        //    // Save the booking
        //    _context.Bookings.Add(booking);
        //    await _context.SaveChangesAsync();

        //    // Create booking details for each seat
        //    foreach (var seat in seatsToReserve)
        //    {
        //        var bookingDetail = new BookingDetail
        //        {
        //            BookingId = booking.BookingId,
        //            SeatNumber = seat.SeatNumber,
        //            IsPaid = true
        //        };
        //        _context.BookingDetails.Add(bookingDetail);
        //    }

        //    // Update flight seat availability in the database
        //    _context.FlightSeats.UpdateRange(seatsToReserve);

        //    // Save changes to the database
        //    await _context.SaveChangesAsync();

        //    return booking.BookingId;
        //}


        public async Task<int> BookTicket(Booking booking, List<string> seatIds)
        {
            // Retrieve the flight with its seats
            var flight = await _context.Flights
                .Include(f => f.FlightSeats)
                .FirstOrDefaultAsync(f => f.FlightId == booking.FlightId);

            if (flight == null)
                throw new KeyNotFoundException("Flight not found.");

            // Validate the requested seat IDs
            var requestedSeats = flight.FlightSeats
                .Where(fs => seatIds.Contains(fs.SeatNumber) && fs.IsAvailable)
                .ToList();

            if (requestedSeats.Count != seatIds.Count)
                throw new InvalidOperationException("One or more requested seats are unavailable or invalid.");

            // Reserve the seats by marking them as unavailable
            requestedSeats.ForEach(seat => seat.IsAvailable = false);

            // Calculate the total price and set the number of seats in the booking
            booking.TotalPrice = flight.PricePerSeat * seatIds.Count;
            booking.NumberOfSeats = seatIds.Count; // Fix: Set NumberOfSeats here

            // Save the booking
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Create booking details for each reserved seat
            foreach (var seat in requestedSeats)
            {
                var bookingDetail = new BookingDetail
                {
                    BookingId = booking.BookingId,
                    SeatNumber = seat.SeatNumber,
                    IsPaid = true
                };
                _context.BookingDetails.Add(bookingDetail);
            }

            // Update the database with the reserved seats
            _context.FlightSeats.UpdateRange(requestedSeats);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return booking.BookingId;
        }



        public async Task<IEnumerable<Booking>> GetBookingHistory(string userId)
        {
            return await Task.FromResult(_context.Bookings
                .Where(b => b.UserId == userId)
                .ToList());
        }


        public async Task CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null) throw new KeyNotFoundException("Booking not found.");

            // Retrieve related booking details for seats booked under this booking
            var bookingDetails = _context.BookingDetails
                .Where(bd => bd.BookingId == bookingId)
                .ToList();

            // Retrieve the corresponding flight seats and mark them as available
            var seatNumbers = bookingDetails.Select(bd => bd.SeatNumber).ToList();
            var flightSeats = _context.FlightSeats
                .Where(fs => fs.FlightId == booking.FlightId && seatNumbers.Contains(fs.SeatNumber))
                .ToList();

            // Mark the seats as available
            flightSeats.ForEach(fs => fs.IsAvailable = true);


            // Remove booking and details
            _context.BookingDetails.RemoveRange(bookingDetails);
            _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<BookingRetrievalDto>> ListAllBooking(string flightOwnerId)
        {
            // Fetch all bookings for the specified flight owner, including the user name and flight details
            var bookings = await _context.Bookings
                .Include(b => b.Flight)               
                .Include(b => b.User)                  
                .Where(b => b.Flight.FlightOwnerId == flightOwnerId)   // Filter by flight owner
                .ToListAsync();

            // Map the data to the (BookingRetrievalDto)
            var bookingDtos = bookings.Select(b => new BookingRetrievalDto
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                NumberOfSeats = b.NumberOfSeats,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                FlightNumber = b.Flight.FlightNumber, 
                Origin = b.Flight.Origin,             
                Destination = b.Flight.Destination,   
                UserName = b.User.UserName          
            }).ToList();

            return bookingDtos;
        }

        public async Task<IEnumerable<BookingRetrievalDto>> ListAllBookingsForAdmin()
        {
            // Fetch all bookings across all flights
            var bookings = await _context.Bookings
                .Include(b => b.Flight)                
                .Include(b => b.User)                
                .ToListAsync();

            // Map the data to the DTO format (BookingRetrievalDto)
            var bookingDtos = bookings.Select(b => new BookingRetrievalDto
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                NumberOfSeats = b.NumberOfSeats,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                FlightNumber = b.Flight.FlightNumber, 
                Origin = b.Flight.Origin,             
                Destination = b.Flight.Destination,   
                UserName = b.User.UserName            
            }).ToList();

            return bookingDtos;
        }


    }
}
