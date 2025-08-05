using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
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

    public class RevenueExpertTest
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
            AccId = "expert001"
        };

        private ExpertRevenueDTO GetFakeRevenueData() => new ExpertRevenueDTO
        {
            ExpertId = "expert001",
            TotalRevenue = 500,
            CommissionRevenue = 50,
            TotalServicesProvided = 5,
            MonthlyRevenue = new Dictionary<string, decimal> { { "2025-07", 500 } },
            MonthlyCommission = new Dictionary<string, decimal> { { "2025-07", 50 } },
            TopServiceNames = new List<string> { "Tư vấn mùa vụ", "Phân tích đất", "Bón phân" },
            DailyRevenue = new Dictionary<string, decimal> { { "2025-07-27", 300 }, { "2025-07-28", 200 } },
            DailyCommission = new Dictionary<string, decimal> { { "2025-07-27", 30 }, { "2025-07-28", 20 } }
        };

        [Test]
        public async Task GetRevenueByExpert_Unauthenticated_ReturnsBadRequest()
        {
            // Arrange
            _authMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.GetRevenueByExpert();

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.AreEqual("accId is required.", badRequest.Value);
        }

        [Test]
        public async Task GetRevenueByExpert_HasNoBooking_ReturnsEmptyDTO()
        {
            // Arrange
            _authMock.Setup(x => x.GetDataFromToken()).Returns(GetFakeUser());
            _statisticServiceMock.Setup(s => s.GetRevenueByExpertAsync("expert001", null, null))
                                 .ReturnsAsync(new ExpertRevenueDTO());

            // Act
            var result = await _controller.GetRevenueByExpert();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var dto = okResult.Value as ExpertRevenueDTO;
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.TotalRevenue);
            Assert.AreEqual(0, dto.CommissionRevenue);
        }

        [Test]
        public async Task GetRevenueByExpert_WithData_ReturnsRevenueDTO()
        {
            // Arrange
            var user = GetFakeUser();
            var data = GetFakeRevenueData();

            _authMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _statisticServiceMock.Setup(s => s.GetRevenueByExpertAsync(user.AccId, null, null))
                                 .ReturnsAsync(data);

            // Act
            var result = await _controller.GetRevenueByExpert();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var dto = okResult.Value as ExpertRevenueDTO;
            Assert.IsNotNull(dto);
            Assert.AreEqual(500, dto.TotalRevenue);
            Assert.AreEqual(50, dto.CommissionRevenue);
            Assert.AreEqual(5, dto.TotalServicesProvided);
            Assert.AreEqual("expert001", dto.ExpertId);
        }
    }
}
