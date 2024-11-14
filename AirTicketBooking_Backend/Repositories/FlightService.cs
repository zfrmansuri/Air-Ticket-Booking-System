using AirTicketBooking_Backend.Authentication;
using AirTicketBooking_Backend.Data;
using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AirTicketBooking_Backend.Repositories
{
    public class FlightService : IFlightService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;


        public FlightService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task AddFlight(Flight flight)
        {
            if (flight == null) throw new ArgumentNullException(nameof(flight));
            await _dbContext.Flights.AddAsync(flight);
            await _dbContext.SaveChangesAsync();

            // Generate seats based on the availableSeats count
            var seats = GenerateSeats(flight.FlightId, flight.AvailableSeats);

            await _dbContext.FlightSeats.AddRangeAsync(seats);
            await _dbContext.SaveChangesAsync();
        }

        // Method to generate seats
        private List<FlightSeat> GenerateSeats(int flightId, int availableSeats)
        {
            var seats = new List<FlightSeat>();
            var rows = new[] { "A", "B", "C", "D" };  // Seat letters

            for (int i = 1; i <= availableSeats; i++)
            {
                var rowIndex = (i - 1) / rows.Length; // To loop over numbers for rows
                var seatLetter = rows[(i - 1) % rows.Length]; // Cycle through A, B, C, D
                var seatNumber = $"{seatLetter}{rowIndex + 1}";

                seats.Add(new FlightSeat
                {
                    FlightId = flightId,
                    SeatNumber = seatNumber,
                    IsAvailable = true
                });
            }

            return seats;
        }

        public async Task UpdateFlight(int flightId, FlightDto updatedFlight, string userId)
        {
            //Retrive the flight from the databse
            var existingFlight = await _dbContext.Flights.FindAsync(flightId);
            if (existingFlight == null) throw new KeyNotFoundException("Flight not found");

            // Retrieve the current user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new UnauthorizedAccessException("User not found");

            // Retrieve user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null) throw new InvalidOperationException("Unable to retrieve user roles");

            //Checking if the user is Admin
            bool isAdmin = userRoles.Contains("Admin");

            //throw error when user is not Admin neither Owner of existingFlight
            if (!isAdmin && existingFlight.FlightOwnerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this flight.");
            }

            existingFlight.FlightNumber = updatedFlight.FlightNumber;
            existingFlight.Origin = updatedFlight.Origin;
            existingFlight.Destination = updatedFlight.Destination;
            existingFlight.DepartureDate = updatedFlight.DepartureDate;
            existingFlight.AvailableSeats = updatedFlight.AvailableSeats;
            existingFlight.PricePerSeat = updatedFlight.PricePerSeat;

            _dbContext.Flights.Update(existingFlight);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveFlight(int flightId)
        {
            var flight = await _dbContext.Flights.FindAsync(flightId);
            if (flight == null) throw new KeyNotFoundException("Flight not found");

            _dbContext.Flights.Remove(flight);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Flight>> SearchFlights(string origin, string destination, DateTime? date)
        {
            var query = _dbContext.Flights.AsQueryable();

            if (!string.IsNullOrEmpty(origin))
                query = query.Where(f => f.Origin.Contains(origin));

            if (!string.IsNullOrEmpty(destination))
                query = query.Where(f => f.Destination.Contains(destination));

            if (date.HasValue)
                query = query.Where(f => f.DepartureDate.Date == date.Value.Date);

            return await query.ToListAsync();
        }

        public async Task<Flight> GetFlightDetails(int flightId)
        {
            var flight = await _dbContext.Flights
                .Include(f => f.FlightSeats) // Include seats if needed
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null) throw new KeyNotFoundException("Flight not found");
            return flight;
        }
    }
}
