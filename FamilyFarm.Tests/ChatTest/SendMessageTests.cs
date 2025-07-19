using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.ChatTest
{
    [TestFixture]
    public class SendMessageTests
    {
        private Mock<IChatService> _chatServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private ChatController _controller;

        [SetUp]
        public void Setup()
        {
            _chatServiceMock = new Mock<IChatService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _controller = new ChatController(_chatServiceMock.Object, _authenServiceMock.Object);
        }

        [Test]
        public async Task SendMessage_Authenticated_ValidReceiverId_TextMessage_ReturnsSuccess()
        {
            // Arrange
            var accId = "acc01";
            var request = new SendMessageRequestDTO
            {
                ReceiverId = "681370da5908b0f4fb0cd0f9",
                Message = "Hello"
            };
            var expectedResponse = new SendMessageResponseDTO
            {
                Success = true,
                Message = "Message sent successfully."
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.SendMessageAsync(accId, request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as SendMessageResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Message sent successfully.", response.Message);
        }

        [Test]
        public async Task SendMessage_Authenticated_ValidReceiverId_ValidImage_ReturnsSuccess()
        {
            // Arrange
            var accId = "acc01";
            var request = new SendMessageRequestDTO
            {
                ReceiverId = "681370da5908b0f4fb0cd0f9",
                File = new Mock<IFormFile>().Object
            };
            var expectedResponse = new SendMessageResponseDTO
            {
                Success = true,
                Message = "Message sent successfully."
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.SendMessageAsync(accId, request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as SendMessageResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Message sent successfully.", response.Message);
        }

        [Test]
        public async Task SendMessage_Authenticated_ValidReceiverId_FileUploadFails_ReturnsBadRequest()
        {
            // Arrange
            var accId = "acc01";
            var request = new SendMessageRequestDTO
            {
                ReceiverId = "681370da5908b0f4fb0cd0f9",
                File = new Mock<IFormFile>().Object
            };
            var expectedResponse = new SendMessageResponseDTO
            {
                Success = false,
                Message = "File upload failed: Some error"
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.SendMessageAsync(accId, request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as SendMessageResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("File upload failed: Some error", response.Message);
        }

        [Test]
        public async Task SendMessage_Authenticated_InvalidReceiverId_ReturnsBadRequest()
        {
            // Arrange
            var accId = "acc01";
            var request = new SendMessageRequestDTO
            {
                ReceiverId = "bcd"
            };
            var expectedResponse = new SendMessageResponseDTO
            {
                Success = false,
                Message = "Receiver not found."
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.SendMessageAsync(accId, request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as SendMessageResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Receiver not found.", response.Message);
        }

        [Test]
        public async Task SendMessage_Authenticated_NullRequest_ReturnsBadRequest()
        {
            // Arrange
            var accId = "acc01";
            SendMessageRequestDTO request = null;

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("No message data provided.", badRequestResult.Value);
        }

        [Test]
        public async Task SendMessage_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var request = new SendMessageRequestDTO
            {
                ReceiverId = "681370da5908b0f4fb0cd0f9",
                Message = "Hello"
            };
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
            Assert.AreEqual("Invalid token or user not found.", unauthorizedResult.Value);
        }

        [TearDown]
        public void TearDown()
        {
            _chatServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
