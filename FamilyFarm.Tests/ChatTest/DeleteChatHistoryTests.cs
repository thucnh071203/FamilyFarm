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
    public class DeleteChatHistoryTests
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
        public async Task DeleteChatHistory_Authenticated_ValidChatId_ReturnsSuccess()
        {
            // Arrange
            var chatId = "681f6d641e183002a417f28c";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _chatServiceMock.Setup(s => s.DeleteChatHistoryAsync(chatId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteChatHistory(chatId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Chat history deleted successfully.", okResult.Value);
        }

        [Test]
        public async Task DeleteChatHistory_Authenticated_NonExistentChatId_ReturnsSuccess()
        {
            // Arrange
            var chatId = "681f6d641e183002a417f299";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _chatServiceMock.Setup(s => s.DeleteChatHistoryAsync(chatId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteChatHistory(chatId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Chat history deleted successfully.", okResult.Value);
        }

        [Test]
        public async Task DeleteChatHistory_Authenticated_InvalidChatId_ReturnsNotFound()
        {
            // Arrange
            var chatId = "abc";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });

            // Act
            var result = await _controller.DeleteChatHistory(chatId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No chats found.", notFoundResult.Value);
        }

        [Test]
        public async Task DeleteChatHistory_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var chatId = "681f6d641e183002a417f28c";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.DeleteChatHistory(chatId);

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
