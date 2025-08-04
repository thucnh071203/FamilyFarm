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
    public class TotalExpertFarmer
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
        public async Task CountByRole_ReturnsOkResultWithData()
        {
            // Arrange
            var mockData = new TotalFarmerExpertDTO<Dictionary<string, int>>
            {
                IsSuccess = true,
                Message = "Successfully total expert/farmer.",
                Data = new Dictionary<string, int>
            {
                { "Farmer", 120 },
                { "Expert", 80 }
            }
            };

            _accountServiceMock
                .Setup(x => x.GetTotalByRoleIdsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _controller.CountByRole();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value as TotalFarmerExpertDTO<Dictionary<string, int>>;
            Assert.IsNotNull(value);
            Assert.IsTrue(value.IsSuccess);
            Assert.AreEqual(120, value.Data["Farmer"]);
            Assert.AreEqual(80, value.Data["Expert"]);
        }

        [Test]
        public async Task CountByRole_WhenServiceFails_Returns500()
        {
            // Arrange
            var roleIds = new List<string> { "68007b0387b41211f0af1d56", "68007b2a87b41211f0af1d57" };

            _accountServiceMock.Setup(s => s.GetTotalByRoleIdsAsync(roleIds)).ThrowsAsync(new Exception("Some error"));

            var controller = new StatisticsController(_statisticServiceMock.Object, _accountServiceMock.Object, _authMock.Object);

            // Act
            var result = await controller.CountByRole();

            // Assert
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);

            // ✅ Truy cập dynamic đúng cách
            var data = objectResult.Value;
            var errorProp = data.GetType().GetProperty("error");
            var errorValue = errorProp?.GetValue(data) as string;

            Assert.AreEqual("Fail to load", errorValue);
        }



    }
}
