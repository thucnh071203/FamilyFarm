using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.StatisticExpert
{
    public class GetByStatusExpert
    {
        private Mock<IStatisticService> _statisticServiceMock;
        private Mock<IAuthenticationService> _authMock;
        private Mock<IAccountService> _accountMock;
        private StatisticsController _controller;

        [SetUp]
        public void Setup()
        {
            _statisticServiceMock = new Mock<IStatisticService>();
            _authMock = new Mock<IAuthenticationService>();
            _accountMock = new Mock<IAccountService>();
            _controller = new StatisticsController(_statisticServiceMock.Object, _accountMock.Object, _authMock.Object);
        }

        private UserClaimsResponseDTO GetFakeUser() => new UserClaimsResponseDTO
        {
            AccId = "6829a654db32945979a1f7f9"
        };

        private List<BookingService> GetFakeBookings() => new List<BookingService>
        {
            new BookingService
            {
                ServiceId = "685829cc163e571a0e870ad1",
                ExpertId = "6829a654db32945979a1f7f9",
                BookingServiceStatus = "Pending",
                IsDeleted = false
            }
        };

        // UTC001: Login hợp lệ, status có dữ liệu => Trả về danh sách
        [Test]
        public async Task UTC001_GetByStatus_WithValidTokenAndData_ReturnsBookingList()
        {
            _authMock.Setup(x => x.GetDataFromToken()).Returns(GetFakeUser());
            _statisticServiceMock.Setup(s => s.GetBookingsByStatusAsync("6829a654db32945979a1f7f9", "Pending"))
                                 .ReturnsAsync(GetFakeBookings());

            var result = await _controller.GetByStatus("Pending");

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            var data = ok.Value as List<BookingService>;
            Assert.IsNotNull(data);
            Assert.IsNotEmpty(data);
        }

        // UTC002: Login hợp lệ, status không có dữ liệu => Trả về danh sách rỗng
        [Test]
        public async Task UTC002_GetByStatus_WithValidTokenButNoData_ReturnsEmptyList()
        {
            _authMock.Setup(x => x.GetDataFromToken()).Returns(GetFakeUser());
            _statisticServiceMock.Setup(s => s.GetBookingsByStatusAsync("6829a654db32945979a1f7f9", "Pending"))
                                 .ReturnsAsync(new List<BookingService>());

            var result = await _controller.GetByStatus("Pending");

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            var data = ok.Value as List<BookingService>;
            Assert.IsNotNull(data);
            Assert.IsEmpty(data);
        }

        // UTC003: User không login => Trả về lỗi thiếu accId
        [Test]
        public async Task UTC003_GetByStatus_Unauthenticated_ReturnsBadRequest()
        {
            _authMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.GetByStatus("Pending");

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("thiếu accId", badRequest.Value);
        }

        // UTC004: Token sai => Trả về lỗi thiếu accId (vì không resolve được accId)
        [Test]
        public async Task UTC004_GetByStatus_InvalidToken_ReturnsBadRequest()
        {
            _authMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = null });

            var result = await _controller.GetByStatus("Pending");

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("thiếu accId", badRequest.Value);
        }
    }
}
