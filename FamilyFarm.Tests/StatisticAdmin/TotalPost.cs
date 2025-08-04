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
        public class TotalPost
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

            private UserClaimsResponseDTO GetFakeAdminUser() => new UserClaimsResponseDTO
            {
                AccId = "admin-id-123"
            };

            // UTC001: Đăng nhập hợp lệ và có dữ liệu => trả về số lượng dịch vụ đã đặt
            [Test]
            public async Task UTC001_GetTotalPosts_WithData_ReturnsCount()
            {
                // Arrange
                long expectedCount = 42;
                _statisticServiceMock.Setup(s => s.GetTotalPostCountAsync()).ReturnsAsync(expectedCount);

                // Act
                var result = await _controller.GetTotalPosts();

                // Assert
                Assert.IsInstanceOf<OkObjectResult>(result);
                var okResult = result as OkObjectResult;

                // Ép kiểu về object cụ thể
                var value = okResult.Value;
                var prop = value.GetType().GetProperty("totalPosts");
                var count = (long)prop.GetValue(value);

                Assert.AreEqual(expectedCount, count);
            }


            // UTC002: Đăng nhập hợp lệ nhưng không có dữ liệu => trả về thông báo rỗng
            [Test]
            public async Task UTC002_GetTotalBookedServices_NoData_ReturnsZero()
            {
                // Arrange
                _authMock.Setup(a => a.GetDataFromToken()).Returns(GetFakeAdminUser());
                _statisticServiceMock.Setup(s => s.GetTotalPostCountAsync()).ReturnsAsync(0);

                // Act
                var result = await _controller.GetTotalPosts();

                // Assert
                Assert.IsInstanceOf<OkObjectResult>(result);
                var okResult = result as OkObjectResult;
                Assert.IsNotNull(okResult);
                Assert.IsNotNull(okResult.Value);

                // Lấy giá trị "totalPosts" từ anonymous object
                var value = okResult.Value;
                var prop = value.GetType().GetProperty("totalPosts");
                var count = (long)prop.GetValue(value);

                Assert.AreEqual(0, count);
            }



            // UTC003: Không đăng nhập => trả về lỗi
            [Test]
            public async Task UTC003_GetTotalBookedServices_Unauthenticated_ReturnsDefaultTotal()
            {
                _authMock.Setup(a => a.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

                // Act
                var result = await _controller.GetTotalPosts();

                Assert.IsInstanceOf<OkObjectResult>(result);
                var okResult = result as OkObjectResult;

                var resultValue = okResult.Value;
                var totalPostsProp = resultValue?.GetType().GetProperty("totalPosts")?.GetValue(resultValue, null);

                Assert.AreEqual(0, totalPostsProp);
            }


            // UTC004: Token không hợp lệ (AccId null) => trả về lỗi
            [Test]
            public async Task UTC004_GetTotalBookedServices_InvalidToken_ReturnsOk()
            {
                _authMock.Setup(a => a.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = null });

                var result = await _controller.GetTotalPosts();

                Assert.IsInstanceOf<OkObjectResult>(result);
                var okResult = result as OkObjectResult;

                Assert.IsNotNull(okResult.Value); 
            }


        }
    }
