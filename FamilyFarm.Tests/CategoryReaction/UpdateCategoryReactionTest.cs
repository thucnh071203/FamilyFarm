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
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.Tests.CategoryReaction
{
    public class UpdateCategoryReactionTest
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
        public async Task NoToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.UpdateCategoryReaction("some-id", new CategoryReactionDTO());

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        [Test]
        public async Task ReactionNotFound_ShouldReturnNotFound()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO());

            _categoryReactionServiceMock.Setup(x => x.GetByIdAsync("not-exist-id"))
                .ReturnsAsync((FamilyFarm.Models.Models.CategoryReaction?)null); // Reaction không tồn tại

            var result = await _controller.UpdateCategoryReaction("not-exist-id", new CategoryReactionDTO());

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task UpdateFails_ShouldReturnNotFound()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO());

            var existing = new FamilyFarm.Models.Models.CategoryReaction { ReactionName = "OldName", IconUrl = "old.png" };

            _categoryReactionServiceMock.Setup(x => x.GetByIdAsync("some-id"))
                .ReturnsAsync(existing);

            _uploadFileServiceMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>()))
                .ReturnsAsync((FileUploadResponseDTO?)null); // không có ảnh mới

            _categoryReactionServiceMock.Setup(x => x.UpdateAsync("some-id", It.IsAny<FamilyFarm.Models.Models.CategoryReaction>()))
                .ReturnsAsync(false); // update thất bại

            var result = await _controller.UpdateCategoryReaction("some-id", new CategoryReactionDTO
            {
                ReactionName = "NewName"
            });

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        [Test]
        public async Task UpdateSuccess_ShouldReturnOk()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO());

            var existing = new FamilyFarm.Models.Models.CategoryReaction { ReactionName = "OldName", IconUrl = "old.png" };

            _categoryReactionServiceMock.Setup(x => x.GetByIdAsync("some-id"))
                .ReturnsAsync(existing);

            _uploadFileServiceMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>()))
                .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "new.png" });

            _categoryReactionServiceMock.Setup(x => x.UpdateAsync("some-id", It.IsAny<FamilyFarm.Models.Models.CategoryReaction>()))
                .ReturnsAsync(true);

            var mockFile = new Mock<IFormFile>();
            var dto = new CategoryReactionDTO
            {
                ReactionName = "NewName",
                IconUrl = mockFile.Object
            };

            var result = await _controller.UpdateCategoryReaction("some-id", dto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }


    }
}
