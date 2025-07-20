using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.BookingTest
{
    [TestFixture]
    public class ViewListBookingOfFarmerTests
    {
        private Mock<IBookingServiceService> _mockBookingService;
        private Mock<IAuthenticationService> _mockAuthService;
        private BookingServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _mockBookingService = new Mock<IBookingServiceService>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _controller = new BookingServiceController(_mockBookingService.Object, _mockAuthService.Object);
        }

        [Test]
        public async Task UTCID01_ValidTokenAndHasBooking_ShouldReturnOk()
        {
            // Arrange
            var accId = "68007b0387b41211f0af1d56";
            _mockAuthService.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _mockBookingService.Setup(s => s.GetRequestBookingOfFarmer(accId))
                .ReturnsAsync(new BookingServiceResponseDTO
                {
                    Success = true,
                    Data = new List<BookingServiceMapper> { new BookingServiceMapper() }
                });

            // Act
            var result = await _controller.ListRequestBookingOfFarmer();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UTCID02_NotAuthenticated_ShouldReturnUnauthorized()
        {
            // Arrange
            _mockAuthService.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.ListRequestBookingOfFarmer();

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task UTCID03_ValidTokenButNoBooking_ShouldReturnBadRequest()
        {
            // Arrange
            var accId = "68007b0387b41211f0af1d56";
            _mockAuthService.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _mockBookingService.Setup(s => s.GetRequestBookingOfFarmer(accId))
                .ReturnsAsync(new BookingServiceResponseDTO
                {
                    Success = false,
                    Message = "You dont have request to book service!"
                });

            // Act
            var result = await _controller.ListRequestBookingOfFarmer();

            // Assert
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("You dont have request to book service!", badRequest.Value);
        }

        [Test]
        public async Task UTCID04_EmptyFarmerId_ShouldReturnUnauthorized()
        {
            // Arrange
            _mockAuthService.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "" });

            // Act
            var result = await _controller.ListRequestBookingOfFarmer();

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }


    }
}
