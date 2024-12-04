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
        private List<FlightSeat> GenerateSeats(int flightId, int seatCount, int startIndex = 1)
        {
            var seats = new List<FlightSeat>();
            var rows = new[] { "A", "B", "C", "D" };

            for (int i = startIndex; i < startIndex + seatCount; i++)
            {
                var rowIndex = (i - 1) / rows.Length; // Row number
                var seatLetter = rows[(i - 1) % rows.Length]; // Seat letter
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
            // Retrieve the flight from the database
            var existingFlight = await _dbContext.Flights.Include(f => f.FlightSeats).FirstOrDefaultAsync(f => f.FlightId == flightId);
            if (existingFlight == null) throw new KeyNotFoundException("Flight not found");

            // Retrieve the current user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new UnauthorizedAccessException("User not found");

            // Retrieve user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null) throw new InvalidOperationException("Unable to retrieve user roles");

            // Checking if the user is Admin
            bool isAdmin = userRoles.Contains("Admin");

            // Throw error when the user is neither Admin nor the Owner of the existing flight
            if (!isAdmin && existingFlight.FlightOwnerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this flight.");
            }

            // Update flight details
            existingFlight.FlightNumber = updatedFlight.FlightNumber;
            existingFlight.Origin = updatedFlight.Origin;
            existingFlight.Destination = updatedFlight.Destination;
            existingFlight.DepartureDate = updatedFlight.DepartureDate;
            existingFlight.PricePerSeat = updatedFlight.PricePerSeat;

            // Handle seat count changes
            if (existingFlight.AvailableSeats != updatedFlight.AvailableSeats)
            {
                UpdateSeats(existingFlight, updatedFlight.AvailableSeats);
                existingFlight.AvailableSeats = updatedFlight.AvailableSeats;
            }

            _dbContext.Flights.Update(existingFlight);
            await _dbContext.SaveChangesAsync();
        }

        // Helper method to update seats
        private void UpdateSeats(Flight flight, int newSeatCount)
        {
            var currentSeats = flight.FlightSeats.Count;

            if (newSeatCount > currentSeats)
            {
                // Add new seats
                var seatsToAdd = newSeatCount - currentSeats;
                var newSeats = GenerateSeats(flight.FlightId, seatsToAdd, currentSeats + 1);
                _dbContext.FlightSeats.AddRange(newSeats);
            }
            else if (newSeatCount < currentSeats)
            {
                // Remove extra seats
                var seatsToRemove = flight.FlightSeats
                    .OrderByDescending(fs => fs.SeatNumber)
                    .Take(currentSeats - newSeatCount)
                    .ToList();

                _dbContext.FlightSeats.RemoveRange(seatsToRemove);
            }
        }

        //public async Task RemoveFlight(int flightId, string userId, bool isAdmin)
        //{
        //    var flight = await _dbContext.Flights.FindAsync(flightId);
        //    if (flight == null)
        //        throw new KeyNotFoundException("Flight not found");

        //    // Check if the current user is Admin or Owener of the Flight
        //    if (!isAdmin && flight.FlightOwnerId != userId)
        //        throw new UnauthorizedAccessException("You are not authorized to remove this flight.");

        //    _dbContext.Flights.Remove(flight);
        //    await _dbContext.SaveChangesAsync();
        //}

        public async Task RemoveFlight(int flightId, string userId, bool isAdmin)
        {
            var flight = await _dbContext.Flights.Include(f => f.Bookings).FirstOrDefaultAsync(f => f.FlightId == flightId);
            if (flight == null)
                throw new KeyNotFoundException("Flight not found");

            // Check if the current user is Admin or Owner of the Flight
            if (!isAdmin && flight.FlightOwnerId != userId)
                throw new UnauthorizedAccessException("You are not authorized to remove this flight.");

            // Remove related bookings first
            if (flight.Bookings != null && flight.Bookings.Any())
            {
                _dbContext.Bookings.RemoveRange(flight.Bookings);
            }

            // Now remove the flight
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

        public async Task<Flight> GetFlightDetails(int flightId)      //depth exception after removing code from program.cs vivek has given
        {
            var flight = await _dbContext.Flights
                .Include(f => f.FlightSeats) // Include seats if needed
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null) throw new KeyNotFoundException("Flight not found");
            return flight;
        }

        public async Task<IEnumerable<Flight>> GetAllFlights(string ownerId)
        {
            // Check if the ownerId is provided, which should be the case for a FlightOwner
            if (!string.IsNullOrEmpty(ownerId))
            {
                return await _dbContext.Flights
                    .Where(f => f.FlightOwnerId == ownerId)
                    .ToListAsync();
            }

            // If no ownerId is provided, then the user is an Admin, so show all flights
            return await _dbContext.Flights
                //.Include(f => f.FlightSeats) 
                //.Include(f => f.FlightOwner) 
                .ToListAsync();
        }


        public async Task<IEnumerable<Flight>> GetAllFlightsForEveryone()
        {
            try
            {
                // Fetch all flights with related FlightSeats and FlightOwner
                var flights = await _dbContext.Flights
                    .Include(f => f.FlightSeats)
                    .Include(f => f.FlightOwner)
                    .ToListAsync();

                return flights;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("A database error occurred while retrieving flights.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving flights.", ex);
            }
        }

    }
}