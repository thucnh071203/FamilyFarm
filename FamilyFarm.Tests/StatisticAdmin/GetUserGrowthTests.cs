using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.StatisticAdmin
{
    public class GetUserGrowthTests
    {
        private StatisticsController _controller;
        private Mock<IStatisticService> _statisticServiceMock;
        private Mock<IAccountService> _accountServiceMock;
        private Mock<IAuthenticationService> _authMock;

        [SetUp]
        public void Setup()
        {
            _statisticServiceMock = new Mock<IStatisticService>();
            _authMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new StatisticsController(_statisticServiceMock.Object, _accountServiceMock.Object, _authMock.Object);
        }
        [Test]
        public async Task GetUserGrowth_ReturnsOk_WhenValidDates()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-10);
            var toDate = DateTime.UtcNow;

            var expectedData = new TotalFarmerExpertDTO<Dictionary<string, int>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new Dictionary<string, int> { { "2025-07-25", 5 } }
            };

            _accountServiceMock
                .Setup(x => x.GetUserGrowthOverTimeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _controller.GetUserGrowth(fromDate, toDate) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            dynamic value = result.Value;
            Assert.True(value.isSuccess);
            Assert.AreEqual("Success", value.message);
        }

        [Test]
        public async Task GetUserGrowth_ReturnsBadRequest_WhenStartDateAfterEndDate()
        {
            // Arrange
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(-5);

            // Act
            var result = await _controller.GetUserGrowth(fromDate, toDate) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            dynamic value = result.Value;
            Assert.False(value.isSuccess);
            Assert.AreEqual("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.", value.message);
        }

        [Test]
        public async Task GetUserGrowth_UsesDefaultDates_WhenNull()
        {
            // Arrange
            var today = DateTime.Today.AddDays(1); // vì controller tự +1 ngày
            var defaultStart = today.AddDays(-31).ToUniversalTime();

            var expectedData = new TotalFarmerExpertDTO<Dictionary<string, int>>
            {
                IsSuccess = true,
                Message = "Default date test",
                Data = new Dictionary<string, int> { { "2025-07-01", 3 } }
            };

            _accountServiceMock
                .Setup(x => x.GetUserGrowthOverTimeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _controller.GetUserGrowth(null, null) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            dynamic value = result.Value;
            Assert.True(value.isSuccess);
            Assert.AreEqual("Default date test", value.message);
        }
    }
}
