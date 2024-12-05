using AirTicketBooking_Backend.Data;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirTicketBooking_Backend.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            var context = new ApplicationDbContext(options);
            _mockContext = new Mock<ApplicationDbContext>(options);
            _bookingService = new BookingService(context);
        }

        [Test]
        public async Task BookTicket_Should_ThrowException_If_FlightNotFound()
        {
            // Arrange
            var booking = new Booking { FlightId = 1, NumberOfSeats = 2 };

            _mockContext.Setup(c => c.Flights.FindAsync(It.IsAny<object[]>()))
                        .ReturnsAsync((Flight)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _bookingService.BookTicket(booking, new List<string>()));
            Assert.AreEqual("Flight not found.", exception.Message);
        }


        [Test]
        public async Task CancelBooking_Should_ThrowException_If_BookingNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.Bookings.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync((Booking)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _bookingService.CancelBooking(1));
            Assert.AreEqual("Booking not found.", exception.Message);
        }

        
    }

    public static class DbSetMockHelper
    {
        public static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return dbSetMock;
        }
    }
}
