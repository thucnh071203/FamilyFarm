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

namespace FamilyFarm.Tests.CategoryServiceTest
{
    public class DeleteCategoryServiceTest
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
        public async Task DeleteCategoryService_ReturnsOk_WhenDeleteSuccessful()
        {
            // Arrange
            var categoryServiceId = "64f0aa7d9b1f4d2eae89d333";
            var mockAccount = new UserClaimsResponseDTO { AccId = "6809ec13b2377ae3f68810de" };

            var mockResult = new CategoryServiceResponseDTO
            {
                Success = true,
                Message = "Delete successful"
            };

            _authenticationService
                .Setup(x => x.GetDataFromToken())
                .Returns(mockAccount);

            _service
                .Setup(x => x.DeleteCategoryService(categoryServiceId))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.DeleteCategoryService(categoryServiceId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as CategoryServiceResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Delete successful", response.Message);
        }
        [Test]
        public async Task DeleteCategoryService_ReturnsNotFound_WhenCategoryNotFound()
        {
            // Arrange
            var categoryServiceId = "64f0aa7d9b1f4d2eae89d333";
            var mockAccount = new UserClaimsResponseDTO { AccId = "6809ec13b2377ae3f68810de" };

            var mockResult = new CategoryServiceResponseDTO
            {
                Success = false,
                Message = "Category not found"
            };

            _authenticationService
                .Setup(x => x.GetDataFromToken())
                .Returns(mockAccount);

            _service
                .Setup(x => x.DeleteCategoryService(categoryServiceId))
                .ReturnsAsync(mockResult);

            // Act
            var result = await _controller.DeleteCategoryService(categoryServiceId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);

            var response = notFoundResult.Value as CategoryServiceResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Category not found", response.Message);
        }
        [Test]
        public async Task DeleteCategoryService_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            var categoryServiceId = "cat1";

            _authenticationService
                .Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.DeleteCategoryService(categoryServiceId);

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual(401, unauthorized.StatusCode);
            Assert.AreEqual("Invalid token or user not found.", unauthorized.Value);
        }


    }
}
