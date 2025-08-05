using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.StatisticAdmin
{
    public class GetSystemRevenueTests
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

        private RevenueSystemDTO GetSampleRevenueDTO() => new RevenueSystemDTO
        {
            TotalRevenue = 1000000,
            TotalBookings = 10,
            TotalCommission = 900000,
            RevenueByMonth = new Dictionary<string, decimal>
            {
                { "2025-08", 1000000 }
            }
        };

        // ✅ UTC001: Đăng nhập hợp lệ và có dữ liệu => Trả về revenue
        [Test]
        public async Task UTC001_GetSystemRevenue_WithData_ReturnsRevenue()
        {
            var expected = GetSampleRevenueDTO();

            _statisticServiceMock.Setup(s => s.GetSystemRevenueAsync(null, null))
                                 .ReturnsAsync(expected);

            var result = await _controller.GetSystemRevenue(null, null);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.AreEqual(expected, ok.Value);
        }

        // ✅ UTC002: Đăng nhập hợp lệ nhưng không có dữ liệu => trả về message "No data to display"
        [Test]
        public async Task UTC002_GetSystemRevenue_NoData_ReturnsEmptyMessage()
        {
            _statisticServiceMock.Setup(s => s.GetSystemRevenueAsync(null, null))
                                 .ReturnsAsync(new RevenueSystemDTO
                                 {
                                     TotalRevenue = 0,
                                     TotalBookings = 0,
                                     TotalCommission = 0,
                                     RevenueByMonth = new Dictionary<string, decimal>()
                                 });

            var result = await _controller.GetSystemRevenue(null, null);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;

            var value = ok.Value as RevenueSystemDTO;
            Assert.AreEqual(0, value.TotalRevenue);
            Assert.AreEqual(0, value.TotalBookings);
        }

        // ✅ UTC003: Không đăng nhập => hệ thống vẫn trả dữ liệu (vì API không có [Authorize])
        [Test]
        public async Task UTC003_GetSystemRevenue_WithoutLogin_ReturnsRevenue()
        {
            var expected = GetSampleRevenueDTO();

            _statisticServiceMock.Setup(s => s.GetSystemRevenueAsync(null, null))
                                 .ReturnsAsync(expected);

            var result = await _controller.GetSystemRevenue(null, null);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.AreEqual(expected, ok.Value);
        }

        // ✅ UTC004: Token không hợp lệ => hệ thống vẫn trả dữ liệu (vì API không kiểm token)
        [Test]
        public async Task UTC004_GetSystemRevenue_InvalidToken_ReturnsRevenue()
        {
            var expected = GetSampleRevenueDTO();

            _statisticServiceMock.Setup(s => s.GetSystemRevenueAsync(null, null))
                                 .ReturnsAsync(expected);

            var result = await _controller.GetSystemRevenue(null, null);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            Assert.AreEqual(expected, ok.Value);
        }
    }
}
