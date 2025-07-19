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
    public class CreateCommentTest
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
        public async Task CreateComment_Authenticated_ValidPostId_ValidContent_ReturnsSuccess()
        {
            // Arrange
            var request = new CommentRequestDTO
            {
                PostId = "686d162c303be06260573a79",
                Content = "This is a great post!"
            };
            var expectedResponse = new CommentResponseDTO
            {
                Success = true,
                Message = "Comment created successfully",
                Data = new Comment
                {
                    CommentId = "1",
                    AccId = "acc01",
                    PostId = request.PostId,
                    Content = request.Content,
                    CreateAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Create(It.IsAny<CommentRequestDTO>(), "acc01")).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as CommentResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Comment created successfully", response.Message);
            Assert.IsNotNull(response.Data);
        }

        [Test]
        public async Task CreateComment_Authenticated_ValidPostId_EmptyContent_ReturnsBadRequest()
        {
            // Arrange
            var request = new CommentRequestDTO
            {
                PostId = "686d162c303be06260573a79",
                Content = ""
            };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Invalid comment data",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Create(It.IsAny<CommentRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request);

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
        public async Task CreateComment_Authenticated_ValidPostId_NullContent_ReturnsBadRequest()
        {
            // Arrange
            var request = new CommentRequestDTO
            {
                PostId = "686d162c303be06260573a79",
                Content = null
            };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Invalid comment data",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Create(It.IsAny<CommentRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request);

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
        public async Task CreateComment_Authenticated_InvalidPostId_ValidContent_ReturnsBadRequest()
        {
            // Arrange
            var request = new CommentRequestDTO
            {
                PostId = "abc",
                Content = "This is a great post!"
            };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Invalid Post ID or Account ID",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Create(It.IsAny<CommentRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid Post ID or Account ID", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task CreateComment_Authenticated_NullPostId_ValidContent_ReturnsBadRequest()
        {
            // Arrange
            var request = new CommentRequestDTO
            {
                PostId = null,
                Content = "This is a great post!"
            };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Invalid comment data",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.Create(It.IsAny<CommentRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request);

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
        public async Task CreateComment_NotAuthenticated_ValidPostId_ValidContent_ReturnsBadRequest()
        {
            // Arrange
            var request = new CommentRequestDTO
            {
                PostId = "686d162c303be06260573a79",
                Content = "This is a great post!"
            };
            var errorResponse = new CommentResponseDTO
            {
                Success = false,
                Message = "Please Login!",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.Create(request);

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
