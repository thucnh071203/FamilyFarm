using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
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

namespace FamilyFarm.Tests.NotificationTest
{
    [TestFixture]
    public class ViewListNotificationsTests
    {
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private NotificationController _controller;

        [SetUp]
        public void Setup()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _controller = new NotificationController(_notificationServiceMock.Object, _authenServiceMock.Object);
        }

        [Test]
        public async Task ListNotificationsForUser_Authenticated_ValidAccountId_NotificationsExist_ReturnsSuccess()
        {
            // Arrange
            var accId = "681370cd5908b0f4fb0cd0f8";
            var expectedResponse = new ListNotifiResponseDTO
            {
                Success = true,
                Message = "Get list of notifications successfully!",
                UnreadCount = 1,
                Notifications = new List<NotificationDTO>
                {
                    new NotificationDTO
                    {
                        NotifiId = "681f286d750d0f6b29fac99a",
                        Content = "You have a new message",
                        CreatedAt = DateTime.UtcNow,
                        CategoryNotifiId = "category01",
                        CategoryName = "Chat",
                        SenderId = "sender01",
                        SenderName = "John Doe",
                        SenderAvatar = "avatar.jpg",
                        TargetId = "target01",
                        TargetType = "Chat",
                        TargetContent = "Hello",
                        Status = new NotificationStatus { NotifiStatusId = "681f286d750d0f6b29fac99b", NotifiId = "681f286d750d0f6b29fac99a", AccId = accId, IsRead = false }
                    }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _notificationServiceMock.Setup(s => s.GetNotificationsForUserAsync(accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListNotificationsForUser();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListNotifiResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get list of notifications successfully!", response.Message);
            Assert.AreEqual(1, response.UnreadCount);
            Assert.IsNotNull(response.Notifications);
            Assert.AreEqual(1, response.Notifications.Count);
        }

        [Test]
        public async Task ListNotificationsForUser_Authenticated_ValidAccountId_NoNotifications_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var accId = "681370cd5908b0f4fb0cd0f8";
            var expectedResponse = new ListNotifiResponseDTO
            {
                Success = true,
                Message = "No notifications found for user.",
                UnreadCount = 0,
                Notifications = new List<NotificationDTO>()
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _notificationServiceMock.Setup(s => s.GetNotificationsForUserAsync(accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListNotificationsForUser();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListNotifiResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("No notifications found for user.", response.Message);
            Assert.AreEqual(0, response.UnreadCount);
            Assert.IsNotNull(response.Notifications);
            Assert.IsEmpty(response.Notifications);
        }

        [Test]
        public async Task ListNotificationsForUser_Authenticated_ValidAccountId_CategoryNull_ReturnsError()
        {
            // Arrange
            var accId = "681370cd5908b0f4fb0cd0f8";
            var expectedResponse = new ListNotifiResponseDTO
            {
                Success = false,
                Message = "Get list notifications failed!",
                UnreadCount = 0,
                Notifications = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _notificationServiceMock.Setup(s => s.GetNotificationsForUserAsync(accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListNotificationsForUser();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListNotifiResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Get list notifications failed!", response.Message);
            Assert.IsNull(response.Notifications);
        }

        [Test]
        public async Task ListNotificationsForUser_Authenticated_ValidAccountId_SenderNotFound_ReturnsSuccess()
        {
            // Arrange
            var accId = "681370cd5908b0f4fb0cd0f8";
            var expectedResponse = new ListNotifiResponseDTO
            {
                Success = true,
                Message = "Get list of notifications successfully!",
                UnreadCount = 1,
                Notifications = new List<NotificationDTO>
                {
                    new NotificationDTO
                    {
                        NotifiId = "681f286d750d0f6b29fac99a",
                        Content = "You have a new message",
                        CreatedAt = DateTime.UtcNow,
                        CategoryNotifiId = "category01",
                        CategoryName = "Chat",
                        SenderId = null,
                        SenderName = null,
                        SenderAvatar = null,
                        TargetId = "target01",
                        TargetType = "Chat",
                        TargetContent = "Hello",
                        Status = new NotificationStatus { NotifiStatusId = "681f286d750d0f6b29fac99b", NotifiId = "681f286d750d0f6b29fac99a", AccId = accId, IsRead = false }
                    }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _notificationServiceMock.Setup(s => s.GetNotificationsForUserAsync(accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListNotificationsForUser();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListNotifiResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get list of notifications successfully!", response.Message);
            Assert.AreEqual(1, response.UnreadCount);
            Assert.IsNotNull(response.Notifications);
            Assert.AreEqual(1, response.Notifications.Count);
            Assert.IsNull(response.Notifications[0].SenderId);
            Assert.IsNull(response.Notifications[0].SenderName);
            Assert.IsNull(response.Notifications[0].SenderAvatar);
        }

        [Test]
        public async Task ListNotificationsForUser_Authenticated_ValidAccountId_TargetContentNull_ReturnsSuccess()
        {
            // Arrange
            var accId = "681370cd5908b0f4fb0cd0f8";
            var expectedResponse = new ListNotifiResponseDTO
            {
                Success = true,
                Message = "Get list of notifications successfully!",
                UnreadCount = 1,
                Notifications = new List<NotificationDTO>
                {
                    new NotificationDTO
                    {
                        NotifiId = "681f286d750d0f6b29fac99a",
                        Content = "You have a new message",
                        CreatedAt = DateTime.UtcNow,
                        CategoryNotifiId = "category01",
                        CategoryName = "Chat",
                        SenderId = "sender01",
                        SenderName = "John Doe",
                        SenderAvatar = "avatar.jpg",
                        TargetId = "target01",
                        TargetType = "Chat",
                        TargetContent = null,
                        Status = new NotificationStatus { NotifiStatusId = "681f286d750d0f6b29fac99b", NotifiId = "681f286d750d0f6b29fac99a", AccId = accId, IsRead = false }
                    }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _notificationServiceMock.Setup(s => s.GetNotificationsForUserAsync(accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListNotificationsForUser();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListNotifiResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get list of notifications successfully!", response.Message);
            Assert.AreEqual(1, response.UnreadCount);
            Assert.IsNotNull(response.Notifications);
            Assert.AreEqual(1, response.Notifications.Count);
            Assert.IsNull(response.Notifications[0].TargetContent);
        }

        [Test]
        public async Task ListNotificationsForUser_NotAuthenticated_ReturnsNotFound()
        {
            // Arrange
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.ListNotificationsForUser();

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Please login", notFoundResult.Value);
        }

        [TearDown]
        public void TearDown()
        {
            _notificationServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
