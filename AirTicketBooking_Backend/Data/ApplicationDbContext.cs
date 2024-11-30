using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Authentication;

namespace AirTicketBooking_Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<Flight> Flights { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<BookingDetail> BookingDetails { get; set; }
        public virtual DbSet<FlightSeat> FlightSeats { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Flight-FlightOwner (One-to-Many relationship)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.FlightOwner)
                .WithMany(u => u.OwnedFlights)
                .HasForeignKey(f => f.FlightOwnerId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete flights when owner is deleted

            // Booking-User (One-to-Many relationship)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete bookings when user is deleted

            // Booking-Flight (Many-to-One relationship)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Flight)
                .WithMany(f => f.Bookings)
                .HasForeignKey(b => b.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            // BookingDetail-Booking (Many-to-One relationship)
            modelBuilder.Entity<BookingDetail>()
                .HasOne(bd => bd.Booking)
                .WithMany(b => b.BookingDetails)
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // FlightSeat-Flight (Many-to-One relationship)
            modelBuilder.Entity<FlightSeat>()
                .HasOne(fs => fs.Flight)
                .WithMany(f => f.FlightSeats)
                .HasForeignKey(fs => fs.FlightId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
