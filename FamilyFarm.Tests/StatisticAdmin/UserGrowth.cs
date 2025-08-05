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
using System.Text.Json;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.StatisticAdmin
{
    public class UserGrowth
    {
        private Mock<IStatisticService> _statisticServiceMock;
        private StatisticsController _controller;
        private Mock<IAuthenticationService> _authMock;
        private Mock<IAccountService> _accountServiceMock;

        [SetUp]
        public void SetUp()
        {
            _statisticServiceMock = new Mock<IStatisticService>();
            _authMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new StatisticsController(_statisticServiceMock.Object, _accountServiceMock.Object, _authMock.Object);
        }
  
        [Test]
        public async Task TC01_StartDateGreaterThanEndDate_ShouldReturnBadRequest()
        {
            var fromDate = new DateTime(2025, 8, 5);
            var toDate = new DateTime(2025, 8, 1);

            _authMock.Setup(x => x.GetDataFromToken())
                     .Returns(new UserClaimsResponseDTO
                     {
                         AccId = "test"
                     });

            var result = await _controller.GetUserGrowth(fromDate, toDate);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);

            var json = JsonSerializer.Serialize(badRequestResult.Value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            Assert.AreEqual("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.", jsonElement.GetProperty("message").GetString());
        }


        [Test]
        public async Task TC02_NullDates_ShouldReturnLast31DaysGrowth()
        {
            var expectedData = new Dictionary<string, int>
    {
        { DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy"), 3 },
        { DateTime.Today.ToString("dd/MM/yyyy"), 5 }
    };

            var mockResponse = new TotalFarmerExpertDTO<Dictionary<string, int>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = expectedData
            };

            _authMock.Setup(x => x.GetDataFromToken())
                     .Returns(new UserClaimsResponseDTO { AccId = "some-id" });

            _accountServiceMock
                .Setup(x => x.GetUserGrowthOverTimeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(mockResponse);

            var result = await _controller.GetUserGrowth(null, null);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            // 👇 Deserialize to JsonElement
            var json = JsonSerializer.Serialize(okResult.Value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            Assert.IsTrue(jsonElement.GetProperty("isSuccess").GetBoolean());

            var data = jsonElement.GetProperty("data");
            Assert.AreEqual(expectedData.Count, data.EnumerateObject().Count());
        }
        [Test]
        public async Task TC03_ValidDatesWithData_ShouldReturnOkWithGrowthData()
        {
            var fromDate = new DateTime(2024, 1, 1);
            var toDate = new DateTime(2024, 1, 10);

            var expectedData = new Dictionary<string, int>
    {
        { "01/01/2024", 1 },
        { "03/01/2024", 2 },
        { "10/01/2024", 3 }
    };

            var mockResponse = new TotalFarmerExpertDTO<Dictionary<string, int>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = expectedData
            };

            _authMock.Setup(x => x.GetDataFromToken())
                     .Returns(new UserClaimsResponseDTO { AccId = "some-id" });

            _accountServiceMock
                .Setup(x => x.GetUserGrowthOverTimeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(mockResponse);

            var result = await _controller.GetUserGrowth(fromDate, toDate);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var json = JsonSerializer.Serialize(okResult.Value);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            Assert.IsTrue(jsonElement.GetProperty("isSuccess").GetBoolean());

            var data = jsonElement.GetProperty("data");
            Assert.AreEqual(expectedData.Count, data.EnumerateObject().Count());
        }


    }
}
