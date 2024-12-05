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
using System.Threading.Tasks;

namespace AirTicketBooking_Backend.Tests
{
    [TestFixture]
    public class UserServiceTests
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

            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Mock RoleManager
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            // Mock IConfiguration
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("cc916555162bfe357cbadcf8e81fc60ab807c1234d7dd3fb95be62bad0e38471");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            // Initialize the service
            _service = new UsersAuthenticationService(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockConfiguration.Object,
                _mockDbContext.Object
            );
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

        [Test]
        public void GetAllUsersByRole_ShouldThrowException_ForInvalidRole()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetAllUsersByRole("InvalidRole"));
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
