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
    public class GetByTimeExpert
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

        private UserClaimsResponseDTO GetFakeUser() => new UserClaimsResponseDTO
        {
            AccId = "685d8591e40c99a8bdfb646d"
        };

        private Dictionary<string, int> GetFakeBookingData() => new Dictionary<string, int>
        {
            { "01/07/2025", 3 },
            { "02/07/2025", 5 }
        };

        // UTC001: User login, có dữ liệu => Trả về dictionary
        [Test]
        public async Task UTC001_GetBookingStatisticByTime_WithValidData_ReturnsCounts()
        {
            var year = 2025;
            var type = "day";

            _authMock.Setup(x => x.GetDataFromToken()).Returns(GetFakeUser());
            _statisticServiceMock.Setup(x => x.GetCountByDayAllMonthsAsync("685d8591e40c99a8bdfb646d", year))
                                 .ReturnsAsync(GetFakeBookingData());

            var result = await _controller.GetBookingStatisticByTime(year, type);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            var data = ok.Value as Dictionary<string, int>;
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Count);
        }

        // UTC002: User login, KHÔNG có dữ liệu => Trả về dictionary rỗng
        [Test]
        public async Task UTC002_GetBookingStatisticByTime_NoData_ReturnsEmpty()
        {
            var year = 2025;
            var type = "day";

            _authMock.Setup(x => x.GetDataFromToken()).Returns(GetFakeUser());
            _statisticServiceMock.Setup(x => x.GetCountByDayAllMonthsAsync("685d8591e40c99a8bdfb646d", year))
                                 .ReturnsAsync(new Dictionary<string, int>());

            var result = await _controller.GetBookingStatisticByTime(year, type);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            var data = ok.Value as Dictionary<string, int>;
            Assert.IsNotNull(data);
            Assert.IsEmpty(data);
        }

        // UTC003: Không login => trả về BadRequest
        [Test]
        public async Task UTC003_GetBookingStatisticByTime_Unauthenticated_ReturnsBadRequest()
        {
            var year = 2025;
            var type = "day";

            _authMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.GetBookingStatisticByTime(year, type);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = result as BadRequestObjectResult;
            Assert.AreEqual("Missing or invalid query parameters", bad.Value);
        }

        // UTC004: Token không hợp lệ (accId null) => BadRequest
        [Test]
        public async Task UTC004_GetBookingStatisticByTime_InvalidToken_ReturnsBadRequest()
        {
            var year = 2025;
            var type = "day";

            _authMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = null });

            var result = await _controller.GetBookingStatisticByTime(year, type);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = result as BadRequestObjectResult;
            Assert.AreEqual("Missing or invalid query parameters", bad.Value);
        }
    }
}
