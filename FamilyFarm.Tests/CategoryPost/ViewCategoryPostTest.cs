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
    public class ViewCategoryPostTest
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
            CategoryId = "680cebdfac700e1cb4c165cc",
            CategoryName = "Lúa Đông Xuân",
            CategoryDescription = "Lúa vụ đầu năm",
            AccId = "acc001",
            IsDeleted = false
        };
        [Test]
        public async Task GetListCategory_Unauthorized_Returns401()
        {
            _authMock.Setup(a => a.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);
            var result = await _controller.GetListCategory();
            Assert.IsInstanceOf<UnauthorizedResult>(result.Result);
        }

        [Test]
        public async Task GetListCategory_Success_ReturnsOk()
        {
            _authMock.Setup(a => a.GetDataFromToken()).Returns(GetFakeUser());

            _serviceMock.Setup(s => s.GetListCategory()).ReturnsAsync(new CategoryPostResponseDTO
            {
                Success = true,
                Data = new List<Category> { GetFakeCategory() }
            });

            var result = await _controller.GetListCategory();
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task GetCategoryById_NotFound_ReturnsBadRequest()
        {
            _authMock.Setup(a => a.GetDataFromToken()).Returns(GetFakeUser());
            _serviceMock.Setup(s => s.GetCategoryById("invalid")).ReturnsAsync((Category)null);

            var result = await _controller.GetCategoryOfPostById("invalid");
            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

    }
}
