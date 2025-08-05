using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
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
    public class ListRequestBookingTest
    {
        private Mock<IBookingServiceService> _bookingService;
        private Mock<IAuthenticationService> _authServiceMock;
        private BookingServiceController _controller;
       
        [SetUp]
        public void Setup()
        {
           _bookingService = new Mock<IBookingServiceService> ();
            _authServiceMock = new Mock<IAuthenticationService>();           
            _controller = new BookingServiceController(_bookingService.Object, _authServiceMock.Object);
        }
        [Test]
        public async Task ListRequestBookingOfExpert_ReturnsUnauthorized_WhenTokenIsMissing()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.ListRequestBookingOfExpert();

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        [Test]
        public async Task ListRequestBookingOfExpert_ReturnsBadRequest_WhenServiceFails()
        {
            var accId = "6810e3831b27b2917c58d77c";
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _bookingService.Setup(x => x.GetRequestBookingOfExpert(accId))
                .ReturnsAsync(new BookingServiceResponseDTO
                {
                    Success = false,
                    Message = "Something went wrong"
                });

            var result = await _controller.ListRequestBookingOfExpert();

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Something went wrong", badRequestResult.Value);
        }
        [Test]
        public async Task ListRequestBookingOfExpert_ReturnsOk_WhenSuccessful()
        {
            var accId = "6810e3831b27b2917c58d77c";
            var response = new BookingServiceResponseDTO
            {
                Success = true,
                Message = "Success",
                Data = new List<BookingServiceMapper>() 
            };

            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _bookingService.Setup(x => x.GetRequestBookingOfExpert(accId))
                .ReturnsAsync(response);

            var result = await _controller.ListRequestBookingOfExpert();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(response, okResult.Value);
        }

    }
}
