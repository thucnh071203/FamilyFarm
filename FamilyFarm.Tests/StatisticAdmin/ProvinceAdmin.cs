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
    public class ProvinceAdmin
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



        private List<UserByProvinceResponseDTO> GetFakeUserStats() => new List<UserByProvinceResponseDTO>
        {
            new UserByProvinceResponseDTO { Province = "Đà Nẵng", UserCount = 12 },
            new UserByProvinceResponseDTO { Province = "Hà Nội", UserCount = 8 }
        };

        [Test]
        public async Task GetUsersByProvince_ReturnsOkWithData()
        {
            // Arrange
            var fakeData = GetFakeUserStats();
            _statisticServiceMock.Setup(s => s.GetUsersByProvinceAsync()).ReturnsAsync(fakeData);

            // Act
            var result = await _controller.GetUsersByProvince();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var json = JsonSerializer.Serialize(okResult.Value);
            var body = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.IsTrue(body.ContainsKey("isSuccess"));
            Assert.AreEqual(true, body["isSuccess"].GetBoolean());

            Assert.AreEqual("User data by province retrieved successfully.", body["message"].GetString());

            Assert.IsTrue(body.ContainsKey("data"));
            Assert.AreEqual(2, body["data"].GetArrayLength());
        }

        [Test]
        public async Task GetUsersByProvince_ReturnsNotFound()
        {
            // Arrange
            _statisticServiceMock.Setup(s => s.GetUsersByProvinceAsync()).ReturnsAsync((List<UserByProvinceResponseDTO>)null);

            // Act
            var result = await _controller.GetUsersByProvince();

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);

            var json = JsonSerializer.Serialize(notFoundResult.Value);
            var body = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.IsTrue(body.ContainsKey("isSuccess"));
            Assert.AreEqual(false, body["isSuccess"].GetBoolean());

            Assert.AreEqual("No user data found by province.", body["message"].GetString());
            Assert.True(body["data"].ValueKind == JsonValueKind.Null);
        }
    }
}
