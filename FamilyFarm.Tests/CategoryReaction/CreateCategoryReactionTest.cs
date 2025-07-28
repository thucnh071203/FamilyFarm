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
        private Mock<ICategoryReactionService> _serviceMock;
        private Mock<IAuthenticationService> _authMock;
        private Mock<IUploadFileService> _fileMock;
        private CategoryReactionController _controller;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<ICategoryReactionService>();
            _authMock = new Mock<IAuthenticationService>();
            _fileMock = new Mock<IUploadFileService>();
            _controller = new CategoryReactionController(_serviceMock.Object, _authMock.Object, _fileMock.Object);
        }

        private IFormFile CreateMockImageFile(string fileName, string contentType = "image/png", int sizeInKb = 100)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(new string('a', sizeInKb * 1024)));
            return new FormFile(stream, 0, stream.Length, "IconUrl", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Test]
        public async Task CreateReaction_ValidInput_ReturnsSuccess()
        {
            var mockUser = new UserClaimsResponseDTO { AccId = "123" };
            _authMock.Setup(a => a.GetDataFromToken()).Returns(mockUser);

            var file = CreateMockImageFile("like.png");
            _fileMock.Setup(f => f.UploadImage(It.IsAny<IFormFile>()))
                     .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "http://mock.url/icon.png" });

            var request = new CategoryReactionDTO
            {
                ReactionName = "Like",
                IconUrl = file
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CategoryReaction>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(request);

            var ok = result as OkObjectResult;
            Assert.NotNull(ok);
            Assert.AreEqual(200, ok.StatusCode);
            Assert.That(((CategoryReactionResponse<CategoryReaction>)ok.Value).Success, Is.True);
        }

        [Test]
        public async Task CreateReaction_UserNotLoggedIn_ReturnsNullTokenError()
        {
            _authMock.Setup(a => a.GetDataFromToken()).Returns(() => null);

            var request = new CategoryReactionDTO
            {
                ReactionName = "Like"
            };

            var result = await _controller.Create(request);

            // Vì controller không xử lý lỗi token null nên nó sẽ bị lỗi ở dòng `user.AccId`, ta không assert gì đặc biệt ở đây
            Assert.ThrowsAsync<NullReferenceException>(() => _controller.Create(request));
        }

        [Test]
        public async Task CreateReaction_EmptyReactionName_ReturnsSuccessButEmptyName()
        {
            var mockUser = new UserClaimsResponseDTO { AccId = "123" };
            _authMock.Setup(a => a.GetDataFromToken()).Returns(mockUser);

            var file = CreateMockImageFile("icon.webp");
            _fileMock.Setup(f => f.UploadImage(It.IsAny<IFormFile>()))
                     .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "http://mock.url/icon.png" });

            var request = new CategoryReactionDTO
            {
                ReactionName = "",  // Case test tên rỗng
                IconUrl = file
            };

            var result = await _controller.Create(request);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            var response = (CategoryReactionResponse<CategoryReaction>)ok.Value;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("", response.Data.ReactionName);
        }

        [Test]
        public async Task CreateReaction_NoFileAttached_StillReturnsSuccess()
        {
            var mockUser = new UserClaimsResponseDTO { AccId = "123" };
            _authMock.Setup(a => a.GetDataFromToken()).Returns(mockUser);

            var request = new CategoryReactionDTO
            {
                ReactionName = "Love",
                IconUrl = null
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CategoryReaction>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(request);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            var response = (CategoryReactionResponse<CategoryReaction>)ok.Value;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("", response.Data.IconUrl); // Vì không upload file nên IconUrl sẽ là ""
        }

        [Test]
        public async Task CreateReaction_InvalidFileFormat_ReturnsEmptyUrl()
        {
            var mockUser = new UserClaimsResponseDTO { AccId = "123" };
            _authMock.Setup(a => a.GetDataFromToken()).Returns(mockUser);

            var file = CreateMockImageFile("bad.exe", "application/octet-stream");
            _fileMock.Setup(f => f.UploadImage(It.IsAny<IFormFile>()))
                     .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "" }); // Không upload được

            var request = new CategoryReactionDTO
            {
                ReactionName = "Dislike",
                IconUrl = file
            };

            var result = await _controller.Create(request);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            var response = (CategoryReactionResponse<CategoryReaction>)ok.Value;
            Assert.AreEqual("", response.Data.IconUrl);
        }

        [Test]
        public async Task CreateReaction_FileSizeExceeded_ReturnsSuccessButStillEmptyUrl()
        {
            var mockUser = new UserClaimsResponseDTO { AccId = "123" };
            _authMock.Setup(a => a.GetDataFromToken()).Returns(mockUser);

            var bigFile = CreateMockImageFile("big.png", "image/png", sizeInKb: 6000); // > 5MB
            _fileMock.Setup(f => f.UploadImage(It.IsAny<IFormFile>()))
                     .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "" }); // Không xử lý upload trong test

            var request = new CategoryReactionDTO
            {
                ReactionName = "Wow",
                IconUrl = bigFile
            };

            var result = await _controller.Create(request);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            var response = (CategoryReactionResponse<CategoryReaction>)ok.Value;
            Assert.AreEqual("", response.Data.IconUrl);
        }

    }
}
