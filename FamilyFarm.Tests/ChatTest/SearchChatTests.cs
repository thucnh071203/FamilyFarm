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
    public class SearchChatTests
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
        public async Task SearchChatsByFullName_Authenticated_ValidFullName_MatchingChat_ReturnsSuccess()
        {
            // Arrange
            var accId = "acc01";
            var fullName = "Thuc";
            var chats = new List<Chat>
            {
                new Chat
                {
                    ChatId = "681f6d641e183002a417f28c",
                    Acc1Id = accId,
                    Acc2Id = "681370da5908b0f4fb0cd0f9",
                    CreateAt = DateTime.UtcNow
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.SearchChatsByFullNameAsync(accId, fullName)).ReturnsAsync(chats);

            // Act
            var result = await _controller.SearchChatsByFullName(fullName);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as List<Chat>;
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Count);
            Assert.AreEqual("681f6d641e183002a417f28c", response[0].ChatId);
        }

        [Test]
        public async Task SearchChatsByFullName_Authenticated_EmptyFullName_ReturnsBadRequest()
        {
            // Arrange
            var accId = "acc01";
            var fullName = "";

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });

            // Act
            var result = await _controller.SearchChatsByFullName(fullName);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("FullName is required.", badRequestResult.Value);
        }

        [Test]
        public async Task SearchChatsByFullName_Authenticated_NonMatchingFullName_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var accId = "acc01";
            var fullName = "Nonexistentname";
            var chats = new List<Chat>();

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.SearchChatsByFullNameAsync(accId, fullName)).ReturnsAsync(chats);

            // Act
            var result = await _controller.SearchChatsByFullName(fullName);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No chats found!", notFoundResult.Value);
        }

        [Test]
        public async Task SearchChatsByFullName_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var fullName = "Thuc";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.SearchChatsByFullName(fullName);

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
