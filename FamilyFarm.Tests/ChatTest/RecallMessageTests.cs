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

namespace FamilyFarm.Tests.ChatTest
{
    [TestFixture]
    public class RecallMessageTests
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
        public async Task RecallChatMessage_Authenticated_ValidChatDetailId_ReturnsSuccess()
        {
            // Arrange
            var chatDetailId = "68386cdf26380db5d2cc1610";
            var expectedResponse = new ChatDetail
            {
                ChatDetailId = chatDetailId,
                ChatId = "681f6d641e183002a417f28c",
                SenderId = "acc01",
                ReceiverId = "681370da5908b0f4fb0cd0f9",
                Message = "Hello",
                SendAt = DateTime.UtcNow,
                IsRecalled = true
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _chatServiceMock.Setup(s => s.RecallChatDetailByIdAsync(chatDetailId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RecallChatMessage(chatDetailId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ChatDetail;
            Assert.IsNotNull(response);
            Assert.AreEqual(chatDetailId, response.ChatDetailId);
            Assert.IsTrue(response.IsRecalled);
        }

        [Test]
        public async Task RecallChatMessage_Authenticated_InvalidChatDetailId_ReturnsNotFound()
        {
            // Arrange
            var chatDetailId = "abc";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _chatServiceMock.Setup(s => s.RecallChatDetailByIdAsync(chatDetailId)).ReturnsAsync((ChatDetail)null);

            // Act
            var result = await _controller.RecallChatMessage(chatDetailId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No message found!", notFoundResult.Value);
        }

        [Test]
        public async Task RecallChatMessage_Authenticated_NonExistentChatDetailId_ReturnsNotFound()
        {
            // Arrange
            var chatDetailId = "68386cdf26380db5d2cc161a";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _chatServiceMock.Setup(s => s.RecallChatDetailByIdAsync(chatDetailId)).ReturnsAsync((ChatDetail)null);

            // Act
            var result = await _controller.RecallChatMessage(chatDetailId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No message found!", notFoundResult.Value);
        }

        [Test]
        public async Task RecallChatMessage_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var chatDetailId = "68386cdf26380db5d2cc1610";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.RecallChatMessage(chatDetailId);

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
