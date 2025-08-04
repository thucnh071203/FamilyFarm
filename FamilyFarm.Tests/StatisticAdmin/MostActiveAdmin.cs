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

namespace FamilyFarm.Tests.StatisticAdmin
{
    public class MostActiveAdmin
    {
        private StatisticsController _controller;
        private Mock<IStatisticService> _statisticServiceMock;
        private Mock<IAuthenticationService> _authMock;
        private Mock<IAccountService> _accountServiceMock;

        [SetUp]
        public void Setup()
        {
            _statisticServiceMock = new Mock<IStatisticService>();
            _authMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new StatisticsController(_statisticServiceMock.Object, _accountServiceMock.Object, _authMock.Object);
        }

        [Test]
        public async Task UTC001_GetMostActiveMembers_ValidDateRange_ReturnsData()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 12, 31);

            var fakeData = new List<MemberActivityResponseDTO>
    {
        new MemberActivityResponseDTO
        {
            AccId = "acc1",
            AccountName = "Test User",
            AccountAddress = "123 Farm",
            RoleName = "Member",
            TotalPosts = 2,
            TotalComments = 3,
            TotalBookings = 1,
            TotalPayments = 1,
            TotalActivity = 7
        }
    };

            _statisticServiceMock.Setup(s => s.GetMostActiveMembersAsync(start, end)).ReturnsAsync(fakeData);

            // Act
            var result = await _controller.GetMostActiveMembers(start, end);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            var data = ok.Value as List<MemberActivityResponseDTO>;

            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual("acc1", data[0].AccId);
        }
        [Test]
        public async Task UTC002_GetMostActiveMembers_ValidDateRange_NoData_ReturnsEmptyList()
        {
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 12, 31);

            _statisticServiceMock.Setup(s => s.GetMostActiveMembersAsync(start, end)).ReturnsAsync(new List<MemberActivityResponseDTO>());

            var result = await _controller.GetMostActiveMembers(start, end);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            var data = ok.Value as List<MemberActivityResponseDTO>;

            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.Count);
        }
        [Test]
        public async Task UTC003_GetMostActiveMembers_StartDateAfterEndDate_ReturnsBadRequest()
        {
            var start = new DateTime(2024, 12, 31);
            var end = new DateTime(2024, 1, 1);

            var result = await _controller.GetMostActiveMembers(start, end);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = result as BadRequestObjectResult;
            Assert.AreEqual("Invalid date range.", bad.Value);
        }
      



    }
}
