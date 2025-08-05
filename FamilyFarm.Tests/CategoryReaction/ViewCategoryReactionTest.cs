using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic;
using FamilyFarm.Controllers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.Tests.CategoryReaction
{
    public class ViewCategoryReactionTest
    {
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<ICategoryReactionService> _categoryReactionServiceMock;
        private Mock<IUploadFileService> _uploadFileServiceMock;
        private CategoryReactionController _controller;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthenticationService>();
            _categoryReactionServiceMock = new Mock<ICategoryReactionService>();
            _uploadFileServiceMock = new Mock<IUploadFileService>();

            _controller = new CategoryReactionController(_categoryReactionServiceMock.Object, _authServiceMock.Object, _uploadFileServiceMock.Object);

        }

        [Test]
        public async Task GetAll_WithData_ShouldReturnList()
        {
            var reactions = new List<FamilyFarm.Models.Models.CategoryReaction>
            {
                new FamilyFarm.Models.Models.CategoryReaction { CategoryReactionId = "1", ReactionName = "Happy" },
                new FamilyFarm.Models.Models.CategoryReaction { CategoryReactionId = "2", ReactionName = "Sad" }
            };

            _categoryReactionServiceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(reactions);

            var result = await _controller.GetAll();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as CategoryReactionResponse<List<FamilyFarm.Models.Models.CategoryReaction>>;
            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(2, response.Data.Count);
        }

        // TC2 - Token hợp lệ, nhưng không có dữ liệu
        [Test]
        public async Task GetAll_NoData_ShouldReturnEmptyList()
        {
            _categoryReactionServiceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<FamilyFarm.Models.Models.CategoryReaction>());

            var result = await _controller.GetAll();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as CategoryReactionResponse<List<FamilyFarm.Models.Models.CategoryReaction>>;
            Assert.IsTrue(response.IsSuccess);
            Assert.IsEmpty(response.Data);
        }

        // TC3 - Không có token (Authorize) -> test này phải test ở integration level, nhưng vẫn mock nếu muốn
        [Test]
        public async Task GetAll_WithoutToken_ShouldStillReturnOk_IfAuthorizeBypassed()
        {
            // NOTE: Unit test không chạy middleware Authorize
            _categoryReactionServiceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<FamilyFarm.Models.Models.CategoryReaction>());

            var result = await _controller.GetAll();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        // TC4 - Service throw exception
        [Test]
        public void GetAll_ServiceThrowsException_ShouldThrow()
        {
            _categoryReactionServiceMock.Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("DB error"));

            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _controller.GetAll();
            });
        }
    }
}
