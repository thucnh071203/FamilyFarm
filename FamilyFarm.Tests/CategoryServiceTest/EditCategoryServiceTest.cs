using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
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
    public class EditCategoryServiceTest
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
        public async Task UpdateCategoryService_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var categoryServiceId = "64f0aa7d9b1f4d2eae89d333";
            var mockAccount = new UserClaimsResponseDTO { AccId = "6809ec13b2377ae3f68810de" };

            var category = new CategoryService
            {
                CategoryServiceId = "64f0aa7d9b1f4d2eae89d333",
                AccId = "6809ec13b2377ae3f68810de",
                CategoryName = "Test Category",
                CategoryDescription = "Description",
                CreateAt = DateTime.Now,
                IsDeleted = false
            };

            var mockResult = new CategoryServiceResponseDTO
            {
                Success = true,
                Message = "Update successful",
                Data = new List<ServiceMapper>
        {
            new ServiceMapper { categoryService = category }
        }
            };

            _authenticationService
                .Setup(x => x.GetDataFromToken())
                .Returns(mockAccount);

            _service
                .Setup(x => x.UpdateCategoryService(categoryServiceId, category))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.UpdateCategoryService(categoryServiceId, category);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as CategoryServiceResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Test Category", response.Data[0].categoryService.CategoryName);
        }
        [Test]
        public async Task UpdateCategoryService_ReturnsNotFound_WhenUpdateFails()
        {
            // Arrange
            var categoryServiceId = "64f0aa7d9b1f4d2eae89d333";
            var mockAccount = new UserClaimsResponseDTO { AccId = "6809ec13b2377ae3f68810de" };

            var category = new CategoryService
            {
                CategoryServiceId = "64f0aa7d9b1f4d2eae89d333",
                AccId = "6809ec13b2377ae3f68810de",
                CategoryName = "Test Category",
                CategoryDescription = "Description",
                CreateAt = DateTime.Now,
                IsDeleted = false
            };

            var mockResult = new CategoryServiceResponseDTO
            {
                Success = false,
                Message = "Category not found"
            };

            _authenticationService
                .Setup(x => x.GetDataFromToken())
                .Returns(mockAccount);

            _service
                .Setup(x => x.UpdateCategoryService(categoryServiceId, category))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.UpdateCategoryService(categoryServiceId, category);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);

            var response = notFoundResult.Value as CategoryServiceResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Category not found", response.Message);
        }
        [Test]
        public async Task UpdateCategoryService_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            var categoryServiceId = "64f0aa7d9b1f4d2eae89d333";
            var category = new CategoryService
            {
                CategoryServiceId = "64f0aa7d9b1f4d2eae89d333",
                AccId = "6809ec13b2377ae3f68810de",
                CategoryName = "Test Category",
                CategoryDescription = "Description",
                CreateAt = DateTime.Now,
                IsDeleted = false
            };

            _authenticationService
                .Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO)null); // giả lập token sai

            // Act
            var result = await _controller.UpdateCategoryService(categoryServiceId, category);

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual(401, unauthorized.StatusCode);
            Assert.AreEqual("Invalid token or user not found.", unauthorized.Value);
        }

    }
}
