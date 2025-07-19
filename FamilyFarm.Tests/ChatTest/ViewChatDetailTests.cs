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
    public class ViewChatDetailTests
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
        public async Task GetMessages_Authenticated_ValidReceiverId_ChatExists_ReturnsSuccess()
        {
            // Arrange
            var accId = "acc01";
            var receiverId = "681a1010fb6a006c117c3481";
            var skip = 0;
            var take = 20;
            var expectedResponse = new ListChatDetailsResponseDTO
            {
                Success = true,
                Message = "Messages retrieved successfully.",
                TotalMessages = 1,
                ChatDetails = new List<ChatDetail>
                {
                    new ChatDetail
                    {
                        ChatDetailId = "68386cdf26380db5d2cc1610",
                        ChatId = "681f6d641e183002a417f28c",
                        SenderId = accId,
                        ReceiverId = receiverId,
                        Message = "Hello",
                        SendAt = DateTime.UtcNow,
                        IsSeen = false,
                        IsRecalled = false
                    }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.GetChatMessagesAsync(accId, receiverId, skip, take)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMessages(receiverId, skip, take);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListChatDetailsResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Messages retrieved successfully.", response.Message);
            Assert.AreEqual(1, response.TotalMessages);
            Assert.IsNotNull(response.ChatDetails);
            Assert.AreEqual(1, response.ChatDetails.Count);
        }

        [Test]
        public async Task GetMessages_Authenticated_NullReceiverId_ReturnsNotFound()
        {
            // Arrange
            var accId = "acc01";
            var receiverId = "";
            var skip = 0;
            var take = 20;
            var expectedResponse = new ListChatDetailsResponseDTO
            {
                Success = false,
                Message = "No messages found for this chat.",
                TotalMessages = 0,
                ChatDetails = new List<ChatDetail>()
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.GetChatMessagesAsync(accId, receiverId, skip, take)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMessages(receiverId, skip, take);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as ListChatDetailsResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("No messages found for this chat.", response.Message);
            Assert.AreEqual(0, response.TotalMessages);
            Assert.IsNotNull(response.ChatDetails);
            Assert.IsEmpty(response.ChatDetails);
        }

        [Test]
        public async Task GetMessages_Authenticated_ValidReceiverId_NoChatFound_ReturnsNotFound()
        {
            // Arrange
            var accId = "acc01";
            var receiverId = "681a1010fb6a006c117c3481";
            var skip = 0;
            var take = 20;
            var expectedResponse = new ListChatDetailsResponseDTO
            {
                Success = false,
                Message = "No messages found for this chat.",
                TotalMessages = 0,
                ChatDetails = new List<ChatDetail>()
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _chatServiceMock.Setup(s => s.GetChatMessagesAsync(accId, receiverId, skip, take)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMessages(receiverId, skip, take);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as ListChatDetailsResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("No messages found for this chat.", response.Message);
            Assert.AreEqual(0, response.TotalMessages);
            Assert.IsNotNull(response.ChatDetails);
            Assert.IsEmpty(response.ChatDetails);
        }

        [Test]
        public async Task GetMessages_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var receiverId = "681a1010fb6a006c117c3481";
            var skip = 0;
            var take = 20;
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.GetMessages(receiverId, skip, take);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
            var response = unauthorizedResult.Value as ListChatDetailsResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Unauthorized access", response.Message);
        }

        [TearDown]
        public void TearDown()
        {
            _chatServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
