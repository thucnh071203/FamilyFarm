using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.BookingTest
{
    public class RequestBooking
    {
        private Mock<IBookingServiceService> _bookingServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private BookingServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _bookingServiceMock = new Mock<IBookingServiceService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _controller = new BookingServiceController(_bookingServiceMock.Object, _authServiceMock.Object);
        }

        [Test]
        public async Task TC01_UnauthenticatedUser_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.CreateBookingService("test description", "123");

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }

        [Test]
        public async Task TC02_MissingDescriptionOrServiceId_ShouldReturnBadRequest()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123" });

            var result = await _controller.CreateBookingService(null, "serviceId");

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("Data unvalid!", badRequest.Value);
        }

        [Test]
        public async Task TC03_ServiceNotFoundOrFailRequest_ShouldReturnBadRequest()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123" });

            _bookingServiceMock.Setup(x => x.RequestToBookingService("acc123", "invalid-service-id", "desc"))
                .ReturnsAsync((bool?)null);

            var result = await _controller.CreateBookingService("desc", "invalid-service-id");

            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("Cannot booking!", badRequest.Value);
        }

        [Test]
        public async Task TC04_ValidRequest_ShouldReturnOk()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc123" });

            _bookingServiceMock.Setup(x => x.RequestToBookingService("acc123", "valid-service-id", "desc"))
                .ReturnsAsync(true);

            var result = await _controller.CreateBookingService("desc", "valid-service-id");

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Booking service successfully!", okResult.Value);
        }
    }
}
