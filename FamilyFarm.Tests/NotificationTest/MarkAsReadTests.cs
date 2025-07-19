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

namespace FamilyFarm.Tests.NotificationTest
{
    [TestFixture]
    public class MarkAsReadTests
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
        public async Task MarkAsReadByNotificationId_Authenticated_ValidNotifiStatusId_NotificationExists_ReturnsSuccess()
        {
            // Arrange
            var accId = "681370cd5908b0f4fb0cd0f8";
            var notifiStatusId = "681f286d750d0f6b29fac99b";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _notificationServiceMock.Setup(s => s.MarkAsReadByNotificationIdAsync(notifiStatusId)).ReturnsAsync(true);

            // Act
            var result = await _controller.MarkAsReadByNotificationId(notifiStatusId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Notification marked as read", okResult.Value);
        }

        [Test]
        public async Task MarkAsReadByNotificationId_Authenticated_InvalidNotifiStatusId_NotificationNotFound_ReturnsNotFound()
        {
            // Arrange
            var accId = "681370cd5908b0f4fb0cd0f8";
            var notifiStatusId = "abc";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _notificationServiceMock.Setup(s => s.MarkAsReadByNotificationIdAsync(notifiStatusId)).ReturnsAsync(false);

            // Act
            var result = await _controller.MarkAsReadByNotificationId(notifiStatusId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No Notification found or user not authorized!", notFoundResult.Value);
        }

        [Test]
        public async Task MarkAsReadByNotificationId_NotAuthenticated_ReturnsNotFound()
        {
            // Arrange
            var notifiStatusId = "681f286d750d0f6b29fac99b";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.MarkAsReadByNotificationId(notifiStatusId);

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
