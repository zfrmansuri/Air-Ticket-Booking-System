using AirTicketBooking_Backend.Authentication;
using AirTicketBooking_Backend.Data;
using AirTicketBooking_Backend.DTOs;
using AirTicketBooking_Backend.Models;
using AirTicketBooking_Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirTicketBooking_Backend.Tests
{
    [TestFixture]
    public class UsersAuthenticationServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ApplicationDbContext> _mockDbContext;
        private UsersAuthenticationService _service;

        [SetUp]
        public void Setup()
        {
            // Mock DbContext
            _mockDbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            // Mocking UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Mock configuration for JWT
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("TestJwtSecurityKey12345");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            // Create the service with the real in-memory database context
            _service = new UsersAuthenticationService(
                _mockUserManager.Object,
                _mockRoleManager?.Object,  // RoleManager is null for simplicity
                _mockConfiguration.Object,
                _mockDbContext.Object
            );
        }

        [Test]
        public async Task RegisterUser_ShouldRegisterAdminWhenFirstUser()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "AdminUser", Email = "admin@test.com" };

            // Mock DbSet<ApplicationUser> to simulate no users initially
            var mockUsers = new Mock<DbSet<ApplicationUser>>();
            var userList = new List<ApplicationUser>(); // No users initially
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(userList.AsQueryable().Provider);
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(userList.AsQueryable().Expression);
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(userList.AsQueryable().ElementType);
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

            _mockDbContext.Setup(db => db.Users).Returns(mockUsers.Object);

            // Mocking CreateAsync and AddToRoleAsync methods
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RegisterUser(user, "Password123");

            // Assert
            Assert.IsTrue(result.Succeeded);
            _mockUserManager.Verify(m => m.AddToRoleAsync(user, "Admin"), Times.Once);
        }

        [Test]
        public async Task RegisterUser_ShouldRegisterAsUserWhenNotFirst()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "RegularUser", Email = "user@test.com" };

            // Mock DbSet<ApplicationUser> to simulate there being one user already
            var mockUsers = new Mock<DbSet<ApplicationUser>>();
            var userList = new List<ApplicationUser>
            {
                new ApplicationUser { UserName = "ExistingUser", Email = "existing@test.com" }
            }; // Already one user
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(userList.AsQueryable().Provider);
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(userList.AsQueryable().Expression);
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(userList.AsQueryable().ElementType);
            mockUsers.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());

            _mockDbContext.Setup(db => db.Users).Returns(mockUsers.Object);

            // Mocking CreateAsync and AddToRoleAsync methods
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RegisterUser(user, "Password123");

            // Assert
            Assert.IsTrue(result.Succeeded);
            _mockUserManager.Verify(m => m.AddToRoleAsync(user, "User"), Times.Once);
        }

        [Test]
        public async Task Login_ShouldReturnJwtToken_ForValidCredentials()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "TestUser", Email = "user@test.com" };
            _mockUserManager.Setup(m => m.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, "Password123")).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            // Act
            var token = await _service.Login("user@test.com", "Password123");

            // Assert
            Assert.NotNull(token);
            Assert.IsInstanceOf<string>(token);
        }

        [Test]
        public void Login_ShouldThrowException_ForInvalidCredentials()
        {
            // Arrange
            _mockUserManager.Setup(m => m.FindByEmailAsync("invalid@test.com")).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _service.Login("invalid@test.com", "WrongPassword"));
        }

        [Test]
        public async Task EditProfile_ShouldUpdateUserProfile_WhenAuthorized()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "1",
                UserName = "OldUser",
                Email = "old@test.com",
                Gender = "Male",
                Address = "Old Address",
                PhoneNumber = "1234567890"
            };
            var updatedProfile = new EditProfileDto
            {
                UserName = "NewUser",
                Email = "new@test.com",
                Gender = "Female",
                Address = "New Address",
                PhoneNumber = "0987654321"
            };
            _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.EditProfile("1", updatedProfile, "1");

            // Assert
            _mockUserManager.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(u =>
                u.UserName == "NewUser" &&
                u.Email == "new@test.com" &&
                u.Gender == "Female" &&
                u.Address == "New Address" &&
                u.PhoneNumber == "0987654321")), Times.Once);
        }

        [Test]
        public void EditProfile_ShouldThrowException_WhenUnauthorized()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1" };
            _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _service.EditProfile("1", new EditProfileDto(), "2"));
        }

        [Test]
        public async Task DeleteProfile_ShouldRemoveUserAndAssociatedData()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1" };
            _mockUserManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Mocking the DbSets for Flights and Bookings
            var flights = new List<Flight>
            {
                new Flight
                {
                    Destination = "New York",
                    FlightNumber = "NY123",
                    FlightOwnerId = "1",
                    Origin = "Los Angeles"
                }
            };

            var bookings = new List<Booking>
            {
                new Booking
                {
                    Status = "Confirmed",
                    UserId = "1"
                }
            };

            _mockDbContext.Setup(db => db.Flights).Returns(MockDbSet(flights).Object);
            _mockDbContext.Setup(db => db.Bookings).Returns(MockDbSet(bookings).Object);

            // Act
            await _service.DeleteProfile("1", "1");

            // Assert
            _mockUserManager.Verify(m => m.DeleteAsync(user), Times.Once);
            _mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.AtLeastOnce);
        }

        [Test]
        public void GetAllUsersByRole_ShouldThrowException_ForInvalidRole()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetAllUsersByRole("InvalidRole"));
        }

        [Test]
        public async Task GetAllUsersByRole_ShouldReturnUsers_ForValidRole()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { UserName = "User1" },
                new ApplicationUser { UserName = "User2" }
            };
            _mockUserManager.Setup(m => m.GetUsersInRoleAsync("User")).ReturnsAsync(users);

            // Act
            var result = await _service.GetAllUsersByRole("User");

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        // Helper method to mock DbSet<T>
        private static Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}
