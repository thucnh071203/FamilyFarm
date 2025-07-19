using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
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

namespace FamilyFarm.Tests.CommentTest
{
    [TestFixture]
    public class EditCommentTest
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
        public async Task EditComment_Authenticated_Owner_ValidCommentId_ValidContent_ReturnsSuccess()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var request = new CommentRequestDTO { Content = "Updated comment text" };
            var expectedResponse = new CommentResponseDTO
            {
                Success = true,
                Message = "Comment updated successfully",
                Data = new Comment
                {
                    CommentId = commentId,
                    AccId = "acc01",
                    PostId = "686d162c303be06260573a79",
                    Content = request.Content,
                    CreateAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Update(commentId, It.IsAny<CommentRequestDTO>(), "acc01")).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Update(commentId, request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as CommentResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Comment updated successfully", response.Message);
            Assert.IsNotNull(response.Data);
        }

        [Test]
        public async Task EditComment_Authenticated_Owner_ValidCommentId_EmptyContent_ReturnsBadRequest()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var request = new CommentRequestDTO { Content = "" };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Invalid comment data",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Update(commentId, It.Is<CommentRequestDTO>(r => r.Content == ""), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Update(commentId, request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid comment data", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task EditComment_Authenticated_Owner_ValidCommentId_NullContent_ReturnsBadRequest()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var request = new CommentRequestDTO { Content = null };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Invalid comment data",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Update(commentId, It.Is<CommentRequestDTO>(r => r.Content == null), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Update(commentId, request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid comment data", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task EditComment_Authenticated_Owner_NonExistentCommentId_ReturnsNotFound()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cb";
            var request = new CommentRequestDTO { Content = "Updated comment text" };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Comment not found",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Update(commentId, It.IsAny<CommentRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Update(commentId, request);

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
        public async Task EditComment_Authenticated_NonOwner_ValidCommentId_ValidContent_ReturnsNotFound()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var request = new CommentRequestDTO { Content = "Updated comment text" };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Comment not found",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc02" });
            _commentServiceMock.Setup(s => s.Update(commentId, It.IsAny<CommentRequestDTO>(), "acc02")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Update(commentId, request);

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
        public async Task EditComment_NotAuthenticated_ValidCommentId_ValidContent_ReturnsBadRequest()
        {
            // Arrange
            var commentId = "681627117372afdcfd2cbf3e";
            var request = new CommentRequestDTO { Content = "Updated comment text" };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Please Login!",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.Update(commentId, request);

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

        [TearDown]
        public void TearDown()
        {
            _commentServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
