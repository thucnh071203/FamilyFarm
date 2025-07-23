using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.CategoryServiceTest
{
    public class ListCategoryServiceTest
    {
        private Mock<ICategoryServicingService> _service;
        private Mock<IAuthenticationService> _authenticationService;
        private CategoryServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<ICategoryServicingService>();
            _authenticationService = new Mock<IAuthenticationService>();
            _controller = new CategoryServiceController(_service.Object, _authenticationService.Object);
        }

        [Test]
        public async Task GetAllForAdmin_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken())
                 .Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.GetAllForAdmin();

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual(401, unauthorized.StatusCode);
            Assert.AreEqual("Invalid token or user not found.", unauthorized.Value);
        }
        [Test]
        public async Task GetAllForAdmin_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var mockUser = new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" };

            _authenticationService
                .Setup(x => x.GetDataFromToken())
                .Returns(mockUser);

            var mockServiceMapper = new ServiceMapper
            {
                service = new Service
                {
                    ServiceId= "6810bd8bbee7273e0e027974",
                    ServiceName = "name",
                    CategoryServiceId= "64f0aa7d9b1f4d2eae89d333",
                    ProviderId = "6809ec13b2377ae3f68810de",
                    Price = 1000,
                    ServiceDescription = "description",
                    
                }, // tạo mock đơn giản, nếu cần bạn có thể thêm properties
                categoryService = new CategoryService
                {
                    CategoryServiceId = "64f0aa7d9b1f4d2eae89d333",
                    AccId = "6809ec13b2377ae3f68810de",
                    CategoryName = "Test Category",
                    CategoryDescription = "Description",
                    CreateAt = DateTime.Now,
                    IsDeleted = false
                },
                Provider = new MyProfileDTO
                {
                    AccId = "6809ec13b2377ae3f68810de",
                    FullName = "Test User",
                    RoleId = "6809ec13b2377ae3f6881057",
                    Username = "testuser",
                    Email = "test@example.com",
                    City = "Hanoi",
                    Country = "Vietnam",
                    Status = 1,
                    IsFacebook = false
                }
            };

            var successResponse = new CategoryServiceResponseDTO
            {
                Success = true,
                Message = "Success",
                Data = new List<ServiceMapper> { mockServiceMapper }
            };

            _service
                .Setup(x => x.GetAllForAdmin())
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.GetAllForAdmin();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var responseData = okResult.Value as CategoryServiceResponseDTO;
            Assert.IsNotNull(responseData);
            Assert.IsTrue(responseData.Success);
            Assert.AreEqual("Success", responseData.Message);
            Assert.IsNotNull(responseData.Data);
            Assert.AreEqual(1, responseData.Data.Count);
            Assert.AreEqual("Test Category", responseData.Data[0].categoryService.CategoryName);
        }


        [Test]
        public async Task GetAllForAdmin_ReturnsNotFound_WhenServiceResponseIsUnsuccessful()
        {
            // Arrange
            var mockUser = new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(mockUser);

            var response = new CategoryServiceResponseDTO
            {
                Success = false,
                Message = "Failed to fetch data",
                Data = null
            };

            _service.Setup(x => x.GetAllForAdmin())
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllForAdmin();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);

            var resultData = notFoundResult.Value as CategoryServiceResponseDTO;
            Assert.IsFalse(resultData.Success);
            Assert.AreEqual("Failed to fetch data", resultData.Message);
        }


    }
}
