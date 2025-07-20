using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.ServiceTest
{
    [TestFixture]

    public class ViewListServiceTests
    {
        private Mock<IServicingService> _serviceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private ServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IServicingService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _controller = new ServiceController(_serviceMock.Object, _authServiceMock.Object);
        }

        private UserClaimsResponseDTO GetMockUser() => new UserClaimsResponseDTO
        {
            AccId = "6808482a0849665c281db8b8",
            Username = "testuser"
        };

        private ServiceMapper GetSampleServiceMapper() => new ServiceMapper
        {
            service = new Service
            {
                ServiceId = "svc123",
                CategoryServiceId = "cat123",
                ProviderId = "acc456",
                ServiceName = "Tư vấn cây trồng",
                ServiceDescription = "Chi tiết tư vấn",
                Price = 150000,
                Status = 1,
                AverageRate = 4.8m,
                RateCount = 20,
                CreateAt = System.DateTime.UtcNow,
                UpdateAt = null,
                ImageUrl = "img.png",
                IsDeleted = false,
                HaveProcess = true
            }
        };

        // ✅ TC01: Lấy danh sách dịch vụ thành công
        [Test]
        public async Task GetAllServices_ValidRequest_ShouldReturnSuccess()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(GetMockUser());
            var mockResponse = new ServiceResponseDTO
            {
                Success = true,
                Message = "Get all services successfully",
                Count = 1,
                Data = new List<ServiceMapper> { GetSampleServiceMapper() }
            };
            _serviceMock.Setup(x => x.GetAllService()).ReturnsAsync(mockResponse);

            var result = await _controller.GetAllServices() as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(200, result.StatusCode);
                var data = result.Value as ServiceResponseDTO;
                Assert.IsTrue(data!.Success);
                Assert.AreEqual(1, data.Count);
            });
        }

        // ✅ TC02: Không có dữ liệu => Trả về thành công (rỗng)
        [Test]
        public async Task GetAllServices_EmptyList_ShouldReturnSuccessFalse()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(GetMockUser());

            var response = new ServiceResponseDTO
            {
                Success = false,
                Message = "Service list is empty",
                Count = 0,
                Data = null
            };

            _serviceMock.Setup(x => x.GetAllService()).ReturnsAsync(response);

            var result = await _controller.GetAllServices() as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result.StatusCode);
                var data = result.Value as ServiceResponseDTO;
                Assert.IsFalse(data!.Success);
                Assert.AreEqual("Service list is empty", data.Message);
            });
        }

        // ✅ TC03: Không đăng nhập (token null)
        [Test]
        public async Task GetAllServices_MissingToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.GetAllServices() as UnauthorizedObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(401, result.StatusCode);
                Assert.AreEqual("Invalid token or user not found.", result.Value);
            });
        }
    }
}
