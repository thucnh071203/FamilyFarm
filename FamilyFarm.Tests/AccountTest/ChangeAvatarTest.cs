using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using AutoMapper;
using System.Security.Claims;
using FamilyFarm.BusinessLogic;

namespace FamilyFarm.Tests.AccountTest
{
    [TestFixture]
    public class ChangeAvatarTests
    {
        private Mock<IAccountService> _accountServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<IMapper> _mapperMock;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _mapperMock = new Mock<IMapper>();

            _controller = new AccountController(
                _accountServiceMock.Object,
                _authenServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task ChangeAvatar_WithValidTokenAndValidImage_ReturnsSuccess()
        {
            // Arrange
            var accountId = "123";
            var fileMock = new Mock<IFormFile>();
            var content = "Fake image content";
            var fileName = "avatar.png";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            var request = new UpdateAvatarRequesDTO { NewAvatar = fileMock.Object };
            var expectedResponse = new UpdateAvatarResponseDTO
            {
                Message = "Update avatar successfully.",
                Success = true,
                Data = "https://image.link/avatar.png"
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accountId });

            _accountServiceMock.Setup(a => a.ChangeOwnAvatar(accountId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ChangeAvatar(request);
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(expectedResponse, okResult.Value);
        }

        [Test]
        public async Task ChangeAvatar_WithoutLogin_ReturnsUnauthorized()
        {
            // Arrange
            var request = new UpdateAvatarRequesDTO(); // no file needed
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.ChangeAvatar(request);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result.Result);
            Assert.AreEqual(401, ((UnauthorizedObjectResult)result.Result).StatusCode);
        }

        [Test]
        public async Task ChangeAvatar_WithNullFile_ReturnsBadRequest()
        {
            // Arrange
            var accountId = "123";
            var request = new UpdateAvatarRequesDTO { NewAvatar = null };

            _authenServiceMock.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accountId });

            _accountServiceMock.Setup(a => a.ChangeOwnAvatar(accountId, request))
                .ReturnsAsync((UpdateAvatarResponseDTO?)null);

            // Act
            var result = await _controller.ChangeAvatar(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
            Assert.AreEqual(400, ((BadRequestObjectResult)result.Result).StatusCode);
        }

        [Test]
        public async Task ChangeAvatar_WithInvalidFormatFile_ReturnsBadRequest()
        {
            // Arrange
            var accountId = "123";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("malicious.exe");
            fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");
            fileMock.Setup(f => f.Length).Returns(500);

            var request = new UpdateAvatarRequesDTO { NewAvatar = fileMock.Object };

            _authenServiceMock.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accountId });

            _accountServiceMock.Setup(a => a.ChangeOwnAvatar(accountId, request))
                .ReturnsAsync((UpdateAvatarResponseDTO?)null);

            // Act
            var result = await _controller.ChangeAvatar(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task ChangeAvatar_WithOversizeFile_ReturnsBadRequest()
        {
            // Arrange
            var accountId = "123";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.png");
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB

            var request = new UpdateAvatarRequesDTO { NewAvatar = fileMock.Object };

            _authenServiceMock.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accountId });

            _accountServiceMock.Setup(a => a.ChangeOwnAvatar(accountId, request))
                .ReturnsAsync((UpdateAvatarResponseDTO?)null);

            // Act
            var result = await _controller.ChangeAvatar(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [TearDown]
        public void TearDown()
        {
            _accountServiceMock.Reset();
            _authenServiceMock.Reset();
            _mapperMock.Reset();
        }
    }
}
