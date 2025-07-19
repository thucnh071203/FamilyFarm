using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
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
    public class SeenMessageTests
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
        public async Task MarkMessagesAsSeen_Authenticated_ValidChatId_ReturnsSuccess()
        {
            // Arrange
            var accId = "acc01";
            var chatId = "681f6d641e183002a417f28c";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.MarkMessagesAsSeenAsync(chatId, accId)).ReturnsAsync(true);

            // Act
            var result = await _controller.MarkMessagesAsSeen(chatId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Messages marked as seen.", okResult.Value);
        }

        [Test]
        public async Task MarkMessagesAsSeen_Authenticated_InvalidChatId_ReturnsBadRequest()
        {
            // Arrange
            var accId = "acc01";
            var chatId = "abc";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.MarkMessagesAsSeenAsync(chatId, accId)).ReturnsAsync(false);

            // Act
            var result = await _controller.MarkMessagesAsSeen(chatId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Failed to mark messages as seen.", badRequestResult.Value);
        }

        [Test]
        public async Task MarkMessagesAsSeen_Authenticated_NonExistentChatId_ReturnsBadRequest()
        {
            // Arrange
            var accId = "acc01";
            var chatId = "681f6d641e183002a417f288";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.MarkMessagesAsSeenAsync(chatId, accId)).ReturnsAsync(false);

            // Act
            var result = await _controller.MarkMessagesAsSeen(chatId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Failed to mark messages as seen.", badRequestResult.Value);
        }

        [Test]
        public async Task MarkMessagesAsSeen_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var chatId = "681f6d641e183002a417f28c";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.MarkMessagesAsSeen(chatId);

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
