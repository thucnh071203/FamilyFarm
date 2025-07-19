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

namespace FamilyFarm.Tests.CommentTest
{
    [TestFixture]
    public class DeleteCommentTest
    {
        private Mock<ICommentService> _commentServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private CommentController _controller;

        [SetUp]
        public void Setup()
        {
            _commentServiceMock = new Mock<ICommentService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _controller = new CommentController(_commentServiceMock.Object, _authenServiceMock.Object);
        }

        [Test]
        public async Task DeleteComment_Authenticated_Owner_ValidCommentId_ReturnsSuccess()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var expectedResponse = new CommentResponseDTO
            {
                Success = true,
                Message = "Comment deleted successfully",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Delete(commentId, "acc01")).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Delete(commentId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as CommentResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Comment deleted successfully", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task DeleteComment_Authenticated_Owner_NonExistentCommentId_ReturnsNotFound()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cb";
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Comment not found",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Delete(commentId, "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Delete(commentId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Comment not found", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task DeleteComment_Authenticated_NonOwner_ValidCommentId_ReturnsNotFound()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Comment not found",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc02" });
            _commentServiceMock.Setup(s => s.Delete(commentId, "acc02")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Delete(commentId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Comment not found", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task DeleteComment_NotAuthenticated_ValidCommentId_ReturnsBadRequest()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Please Login!",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.Delete(commentId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please Login!", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task DeleteComment_Authenticated_InvalidCommentId_ReturnsBadRequest()
        {
            // Arrange
            var commentId = "abc";
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Invalid Comment ID",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Delete(commentId, "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Delete(commentId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid Comment ID", response.Message);
            Assert.IsNull(response.Data);
        }

        [TearDown]
        public void TearDown()
        {
            _commentServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
