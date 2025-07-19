using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
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
    public class ViewListChatTests
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
        public async Task GetUserChats_Authenticated_ChatExists_ReturnsSuccess()
        {
            // Arrange
            var accId = "acc01";
            var expectedResponse = new ListChatResponseDTO
            {
                Success = true,
                Message = "Chats retrieved successfully.",
                unreadChatCount = 1,
                Chats = new List<ChatDTO>
                {
                    new ChatDTO
                    {
                        ChatId = "681f6d641e183002a417f28c",
                        Acc1Id = accId,
                        Acc2Id = "681370da5908b0f4fb0cd0f9",
                        LastMessageAccId = accId,
                        LastMessage = "Hello",
                        LastMessageAt = DateTime.UtcNow,
                        UnreadCount = 1,
                        Receiver = new MyProfileDTO { AccId = "681370da5908b0f4fb0cd0f9" }
                    }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.GetUserChatsAsync(accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserChats();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListChatResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Chats retrieved successfully.", response.Message);
            Assert.AreEqual(1, response.unreadChatCount);
            Assert.IsNotNull(response.Chats);
            Assert.AreEqual(1, response.Chats.Count);
        }

        [Test]
        public async Task GetUserChats_Authenticated_EmptyChatList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var accId = "acc01";
            var expectedResponse = new ListChatResponseDTO
            {
                Success = false,
                Message = "No chats found for the user.",
                unreadChatCount = 0,
                Chats = new List<ChatDTO>()
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.GetUserChatsAsync(accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserChats();

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as ListChatResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("No chats found for the user.", response.Message);
            Assert.AreEqual(0, response.unreadChatCount);
            Assert.IsNotNull(response.Chats);
            Assert.IsEmpty(response.Chats);
        }

        [Test]
        public async Task GetUserChats_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.GetUserChats();

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
