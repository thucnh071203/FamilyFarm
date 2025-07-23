using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.CategoryPost
{


    [TestFixture]
    public class CreateCategoryPostTest
    {
        private Mock<ICategoryPostService> _serviceMock;
        private Mock<IAuthenticationService> _authMock;
        private CategoryPostController _controller;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<ICategoryPostService>();
            _authMock = new Mock<IAuthenticationService>();
            _controller = new CategoryPostController(_serviceMock.Object, _authMock.Object);
        }

        private UserClaimsResponseDTO GetFakeUser() => new UserClaimsResponseDTO { AccId = "acc001" };

        private Category GetFakeCategory() => new Category
        {
            CategoryId = "cat001",
            CategoryName = "Lúa Đông Xuân",
            CategoryDescription = "Lúa vụ đầu năm",
            AccId = "acc001",
            IsDeleted = false
        };

    
        [Test]
        public async Task CreateCategory_Unauthenticated_Returns401()
        {
            _authMock.Setup(a => a.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);
            var result = await _controller.CreateCategoryOfPost(GetFakeCategory());
            Assert.IsInstanceOf<UnauthorizedResult>(result.Result);
        }

        [Test]
        public async Task CreateCategory_Success_ReturnsOk()
        {
            _authMock.Setup(a => a.GetDataFromToken()).Returns(GetFakeUser());

            _serviceMock.Setup(s => s.Create(It.IsAny<Category>())).ReturnsAsync(new CategoryPostResponseDTO
            {
                Success = true
            });

            var result = await _controller.CreateCategoryOfPost(GetFakeCategory());
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task CreateCategory_Fail_ReturnsBadRequest()
        {
            _authMock.Setup(a => a.GetDataFromToken()).Returns(GetFakeUser());

            _serviceMock.Setup(s => s.Create(It.IsAny<Category>())).ReturnsAsync(new CategoryPostResponseDTO
            {
                Success = false,
                MessageError = "Tên danh mục không được trống"
            });

            var result = await _controller.CreateCategoryOfPost(GetFakeCategory());
            var badResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badResult);
            Assert.AreEqual(400, badResult.StatusCode);
        }

      

    
    }
}
