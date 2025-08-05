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
    public class DeleteCategoryReactionTest
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
        public async Task UT00401_NoToken_ShouldReturnUnauthorized()
        {
            // Không có token
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.DeleteCategoryReaction("680ceb8fac700e1cb4c165cc");

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }

        [Test]
        public async Task UT00402_WithToken_InvalidId_ShouldReturnNotFound()
        {
            // Có token hợp lệ
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO());

            // ID không tồn tại
            _categoryReactionServiceMock.Setup(x => x.DeleteAsync("invalid-id"))
                .ReturnsAsync(false);

            var result = await _controller.DeleteCategoryReaction("invalid-id");

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFound = result as NotFoundObjectResult;
            var response = notFound?.Value as CategoryReactionResponse<FamilyFarm.Models.Models.CategoryReaction>;

            Assert.IsFalse(response?.IsSuccess);
            Assert.AreEqual("No reaction found to delete", response?.Message);
        }

        [Test]
        public async Task UT00403_WithToken_ValidId_ShouldReturnOk()
        {
            // Có token
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO());

            // ID hợp lệ
            _categoryReactionServiceMock.Setup(x => x.DeleteAsync("680ceb8fac700e1cb4c165cc"))
                .ReturnsAsync(true);

            var result = await _controller.DeleteCategoryReaction("680ceb8fac700e1cb4c165cc");

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;
            var response = ok?.Value as CategoryReactionResponse<FamilyFarm.Models.Models.CategoryReaction>;

            Assert.IsTrue(response?.IsSuccess);
            Assert.AreEqual("Delete reaction successfully!", response?.Message);
        }

        [Test]
        public async Task UT00404_NoToken_InvalidId_ShouldReturnUnauthorized()
        {
            // Không có token
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.DeleteCategoryReaction("invalid-id");

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }


    }
}
