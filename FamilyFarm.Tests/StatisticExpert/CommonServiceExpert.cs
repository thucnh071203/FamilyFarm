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

namespace FamilyFarm.Tests.StatisticExpert
{
    public class CommonServiceExpert
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

        // UTC001: Có dữ liệu bài viết
        [Test]
        public async Task UTC001_GetTotalBookedServices_WithData_ReturnsCount()
        {
            // Arrange
            long expectedCount = 42;
            _statisticServiceMock.Setup(s => s.GetTotalPostCountAsync()).ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.GetTotalPosts();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, long>>(json);

            Assert.IsNotNull(dict);
            Assert.AreEqual(expectedCount, dict["totalPosts"]);
        }


        // UTC002: Không có dữ liệu (số bài viết là 0)
        [Test]
        public async Task UTC002_GetTotalPosts_NoData_ReturnsZero()
        {
            // Arrange
            _statisticServiceMock.Setup(s => s.GetTotalPostCountAsync()).ReturnsAsync(0);

            // Act
            var result = await _controller.GetTotalPosts();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic data = okResult.Value;
            Assert.AreEqual(0, data.totalPosts);
        }

        // UTC003: Service trả lỗi (nếu cần)
        [Test]
        public void UTC003_GetTotalPosts_ServiceThrowsException_ReturnsException()
        {
            // Arrange
            _statisticServiceMock.Setup(s => s.GetTotalPostCountAsync()).ThrowsAsync(new System.Exception("Database error"));

            // Act + Assert
            Assert.ThrowsAsync<System.Exception>(async () => await _controller.GetTotalPosts());
        }
    }
}
