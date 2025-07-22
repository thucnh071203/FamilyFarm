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

namespace FamilyFarm.Tests.ReactionTest
{
    [TestFixture]
    public class ReactToPostTests
    {
        private Mock<IReactionService> _reactionServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private ReactionController _controller;

        [SetUp]
        public void Setup()
        {
            _reactionServiceMock = new Mock<IReactionService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _controller = new ReactionController(_reactionServiceMock.Object, _authenServiceMock.Object);
        }

        [Test]
        public async Task ToggleReactionPost_Authenticated_ValidPostId_FirstTimeReaction_ReturnsSuccess()
        {
            // Arrange
            var postId = "686d0b32af04947264717a14";
            var categoryReactionId = "671b9f3b7f8a9c4d3e2b1c01";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reactionServiceMock.Setup(s => s.ToggleReactionAsync(postId, "Post", "acc01", categoryReactionId)).ReturnsAsync(true);

            // Act
            var result = await _controller.ToggleReactionPost(postId, categoryReactionId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Reaction has been toggled.", okResult.Value);
        }

        [Test]
        public async Task ToggleReactionPost_Authenticated_ValidPostId_ToggleSameReaction_SoftDelete_ReturnsSuccess()
        {
            // Arrange
            var postId = "686d0b32af04947264717a14";
            var categoryReactionId = "671b9f3b7f8a9c4d3e2b1c01";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reactionServiceMock.Setup(s => s.ToggleReactionAsync(postId, "Post", "acc01", categoryReactionId)).ReturnsAsync(true);

            // Act
            var result = await _controller.ToggleReactionPost(postId, categoryReactionId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Reaction has been toggled.", okResult.Value);
        }

        [Test]
        public async Task ToggleReactionPost_Authenticated_ValidPostId_ChangeReaction_ReturnsSuccess()
        {
            // Arrange
            var postId = "686d0b32af04947264717a14";
            var categoryReactionId = "671b9f3b7f8a9c4d3e2b1c01";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reactionServiceMock.Setup(s => s.ToggleReactionAsync(postId, "Post", "acc01", categoryReactionId)).ReturnsAsync(true);

            // Act
            var result = await _controller.ToggleReactionPost(postId, categoryReactionId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Reaction has been toggled.", okResult.Value);
        }

        [Test]
        public async Task ToggleReactionPost_Authenticated_InvalidPostId_ReturnsBadRequest()
        {
            // Arrange
            var postId = "686d0b32af04947264717";
            var categoryReactionId = "671b9f3b7f8a9c4d3e2b1c01";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reactionServiceMock.Setup(s => s.ToggleReactionAsync(postId, "Post", "acc01", categoryReactionId)).ReturnsAsync(false);

            // Act
            var result = await _controller.ToggleReactionPost(postId, categoryReactionId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Reaction does not exist or is invalid.", badRequestResult.Value);
        }

        [Test]
        public async Task ToggleReactionPost_Authenticated_InvalidCategoryReactionId_ReturnsBadRequest()
        {
            // Arrange
            var postId = "686d0b32af04947264717a14";
            var categoryReactionId = "685e3a2a257421f1c1f2d217";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reactionServiceMock.Setup(s => s.ToggleReactionAsync(postId, "Post", "acc01", categoryReactionId)).ReturnsAsync(false);

            // Act
            var result = await _controller.ToggleReactionPost(postId, categoryReactionId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Reaction does not exist or is invalid.", badRequestResult.Value);
        }

        [Test]
        public async Task ToggleReactionPost_NotAuthenticated_ValidPostId_ReturnsNotFound()
        {
            // Arrange
            var postId = "686d0b32af04947264717a14";
            var categoryReactionId = "671b9f3b7f8a9c4d3e2b1c01";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.ToggleReactionPost(postId, categoryReactionId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No account found!", notFoundResult.Value);
        }

        [TearDown]
        public void TearDown()
        {
            _reactionServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
