using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Controllers;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.CategoryReaction
{
    public class CreateCategoryReactionTest
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

            var controller = new CategoryReactionController(_categoryReactionServiceMock.Object, _authServiceMock.Object, _uploadFileServiceMock.Object);

        }

        [Test]
        public async Task NoToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            var request = new CategoryReactionDTO
            {
                ReactionName = "Happy"
            };
            var controller = new CategoryReactionController(
    _categoryReactionServiceMock.Object,

    _authServiceMock.Object,
                    _uploadFileServiceMock.Object);
            // Act

            var result = await controller.Create(request);

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }


        [Test]
        public async Task Create_ValidTokenAndValidData_ReturnsSuccess()
        {
            var accId = "acc001";
            var user = new UserClaimsResponseDTO { AccId = accId };

            _authServiceMock.Setup(s => s.GetDataFromToken()).Returns(user);

            var mockFile = new Mock<IFormFile>();
            var content = "fake image content";
            var fileName = "icon.png";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.Length).Returns(stream.Length);

            _uploadFileServiceMock.Setup(s => s.UploadImage(It.IsAny<IFormFile>()))
                              .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "http://image.com/icon.png" });

            var dto = new CategoryReactionDTO
            {
                ReactionName = "Like",
                IconUrl = mockFile.Object
            };

            var controller = new CategoryReactionController(
                _categoryReactionServiceMock.Object,

                _authServiceMock.Object,
                                _uploadFileServiceMock.Object);

            var result = await controller.Create(dto) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var response = result.Value as CategoryReactionResponse<FamilyFarm.Models.Models.CategoryReaction>;
            Assert.IsTrue(response!.IsSuccess);
            Assert.AreEqual("Create reaction successfully!", response.Message);
            Assert.AreEqual("Like", response.Data.ReactionName);
        }

        [Test]
        public async Task Create_NoToken_ReturnsUnauthorized()
        {
            _authServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null!);

            var controller = new CategoryReactionController(
                _categoryReactionServiceMock.Object,

                _authServiceMock.Object,
                                _uploadFileServiceMock.Object);

            var dto = new CategoryReactionDTO
            {
                ReactionName = "Like"
            };

            var result = await controller.Create(dto);

            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        [Test]
        public async Task Create_InvalidFileFormat_ReturnsError()
        {
            var accId = "acc001";
            _authServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("malware.exe");
            mockFile.Setup(f => f.Length).Returns(1024);

            _uploadFileServiceMock.Setup(s => s.UploadImage(It.IsAny<IFormFile>()))
                                  .ThrowsAsync(new Exception("Invalid file format"));

            var dto = new CategoryReactionDTO
            {
                ReactionName = "Like",
                IconUrl = mockFile.Object
            };

            var controller = new CategoryReactionController(
                _categoryReactionServiceMock.Object,
                _authServiceMock.Object,
                _uploadFileServiceMock.Object);

            var result = await controller.Create(dto) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);

            var response = result.Value as CategoryReactionResponse<string>;
            Assert.IsFalse(response!.IsSuccess);
            Assert.AreEqual("Invalid file format", response.Message);
            Assert.IsNull(response.Data);
        }



        [Test]
        public async Task Create_FileTooLarge_ReturnsError()
        {
            var accId = "acc001";
            _authServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("image.png");
            mockFile.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB

            _uploadFileServiceMock.Setup(s => s.UploadImage(It.IsAny<IFormFile>()))
                              .ThrowsAsync(new Exception("File size exceeds limit"));

            var dto = new CategoryReactionDTO
            {
                ReactionName = "Like",
                IconUrl = mockFile.Object
            };

            var controller = new CategoryReactionController(
                _categoryReactionServiceMock.Object,

                _authServiceMock.Object,
                                _uploadFileServiceMock.Object);

            var result = await controller.Create(dto) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }
        [Test]
        public async Task Create_ValidData_NoImage_ReturnsSuccess()
        {
            var accId = "acc001";
            _authServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });

            var dto = new CategoryReactionDTO
            {
                ReactionName = "Like",
                IconUrl = null
            };

            var controller = new CategoryReactionController(
                _categoryReactionServiceMock.Object,

                _authServiceMock.Object,
                                _uploadFileServiceMock.Object);

            var result = await controller.Create(dto) as OkObjectResult;

            Assert.IsNotNull(result);
            var response = result.Value as CategoryReactionResponse<FamilyFarm.Models.Models.CategoryReaction>;

            Assert.IsTrue(response!.IsSuccess);
            Assert.AreEqual("Create reaction successfully!", response.Message);
            Assert.AreEqual("Like", response.Data.ReactionName);
            Assert.AreEqual("", response.Data.IconUrl);
        }




    }
}
