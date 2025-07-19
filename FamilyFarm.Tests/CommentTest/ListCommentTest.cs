using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
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
    public class ListCommentTest
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
        public async Task GetListCommentOfPost_Authenticated_ValidPostId_WithComments_ReturnsSuccess()
        {
            // Arrange
            var postId = "686d0b32af04947264717a14";
            var expectedResponse = new ListCommentResponseDTO
            {
                Success = true,
                Message = "Get list comment successfully.",
                Count = 1,
                Data = new List<CommentMapper>
                {
                    new CommentMapper
                    {
                        Comment = new Comment
                        {
                            CommentId = "1",
                            AccId = "acc01",
                            PostId = postId,
                            Content = "Great post!",
                            CreateAt = DateTime.UtcNow,
                            IsDeleted = false
                        },
                        Account = new MyProfileDTO { AccId = "acc01" }
                    }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.GetAllCommentWithReactionByPost(postId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetListCommentOfPost(postId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListCommentResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get list comment successfully.", response.Message);
            Assert.AreEqual(1, response.Count);
            Assert.IsNotNull(response.Data);
        }

        [Test]
        public async Task GetListCommentOfPost_Authenticated_ValidPostId_NoComments_ReturnsSuccess()
        {
            // Arrange
            var postId = "686d162c303be06260573a79";
            var expectedResponse = new ListCommentResponseDTO
            {
                Success = true,
                Message = "There is no comment for post.",
                Count = 0,
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.GetAllCommentWithReactionByPost(postId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetListCommentOfPost(postId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListCommentResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("There is no comment for post.", response.Message);
            Assert.AreEqual(0, response.Count);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task GetListCommentOfPost_Authenticated_NonExistentPostId_ReturnsNotFound()
        {
            // Arrange
            var postId = "686d162c303be06260573a70";
            var expectedResponse = new ListCommentResponseDTO
            {
                Success = false,
                Message = "Post not found",
                Count = 0,
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _commentServiceMock.Setup(s => s.GetAllCommentWithReactionByPost(postId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetListCommentOfPost(postId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as ListCommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Post not found", response.Message);
            Assert.AreEqual(0, response.Count);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task GetListCommentOfPost_Authenticated_NullPostId_ReturnsBadRequest()
        {
            // Arrange
            string postId = null;

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });

            // Act
            var result = await _controller.GetListCommentOfPost(postId);

            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result);
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TearDown]
        public void TearDown()
        {
            _commentServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
