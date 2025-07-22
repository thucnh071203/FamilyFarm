using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using FamilyFarm.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;

namespace FamilyFarm.Tests.BookingTest
{
    [TestFixture]
    public class ResponseToRequestBookingTests
    {
        private Mock<IBookingServiceRepository> _bookingRepoMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IAccountRepository> _accountRepoMock;
        private Mock<IServiceRepository> _serviceRepoMock;
        private Mock<IHubContext<NotificationHub>> _notificationHubMock;
        private Mock<IHubClients> _hubClientsMock;
        private Mock<IClientProxy> _clientProxyMock;
        private Mock<IPaymentRepository> _paymentRepoMock;
        private Mock<IHubContext<AllHub>> _allHubMock;
        private Mock<INotificationService> _notificationServiceMock;
        private BookingServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _bookingRepoMock = new Mock<IBookingServiceRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _notificationHubMock = new Mock<IHubContext<NotificationHub>>();
            _hubClientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _allHubMock = new Mock<IHubContext<AllHub>>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _notificationServiceMock = new Mock<INotificationService>();
            

            _notificationHubMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
            _hubClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);

            var bookingService = new BookingServiceService(
                _bookingRepoMock.Object,
                _accountRepoMock.Object,
                _serviceRepoMock.Object,
                _notificationHubMock.Object,
                _paymentRepoMock.Object,
                _allHubMock.Object,
                _notificationServiceMock.Object
            );

            _controller = new BookingServiceController(bookingService, _authServiceMock.Object);
        }

        private void SetExpertUser() =>
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "expert123",
                RoleId = "68007b2a87b41211f0af1d57"
            });

        [Test]
        public async Task ResponseBooking_ValidBooking_Pending_ShouldReturnOk()
        {
            SetExpertUser();
            var bookingId = "685d63b7306140451dd63af8";
            _bookingRepoMock.Setup(x => x.GetById(bookingId)).ReturnsAsync(new BookingService
            {
                BookingServiceId = bookingId,
                BookingServiceStatus = "Pending",
                AccId = "farmer001"
            });
            _bookingRepoMock.Setup(x => x.UpdateStatus(It.IsAny<BookingService>())).Returns(Task.CompletedTask);

            var result = await _controller.ExpertAcceptBooking(bookingId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        //[Test]
        //public async Task UTCID02_UserNotAuthenticated_ShouldReturnBadRequest()
        //{
        //    string bookingId = null!;
        //    var result = await _controller.ExpertAcceptBooking(bookingId);
        //    Assert.IsInstanceOf<BadRequestResult>(result);
        //}

        [Test]
        public async Task ResponseBooking_UserNotAuthenticated_ShouldReturnOkNull()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.ExpertAcceptBooking("685d63b7306140451dd63af8");
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task ResponseBooking_BookingIdEmpty_ShouldReturnOkNull()
        {
            SetExpertUser();
            string bookingId = "";
            var result = await _controller.ExpertAcceptBooking(bookingId);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
        }

        [Test]
        public async Task ResponseBooking_UserNotExpert_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO
            {
                AccId = "someuser",
                RoleId = "nonExpert"
            };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.ExpertAcceptBooking("685d63b7306140451dd63af8");
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("User is not expert", badRequest.Value);
        }


        [Test]
        public async Task ResponseBooking_ExceptionThrown_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var bookingId = "685d63b7306140451dd63af8";
            _bookingRepoMock.Setup(x => x.GetById(bookingId)).ReturnsAsync(new BookingService
            {
                BookingServiceId = bookingId,
                BookingServiceStatus = "Pending",
                AccId = "farmer001"
            });

            _bookingRepoMock.Setup(x => x.UpdateStatus(It.IsAny<BookingService>())).ThrowsAsync(new Exception("Database error"));

            var result = await _controller.ExpertAcceptBooking(bookingId);
            Assert.IsInstanceOf<BadRequestResult>(result);
        }
    }
}
