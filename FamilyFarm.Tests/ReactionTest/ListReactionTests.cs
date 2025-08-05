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

namespace FamilyFarm.Tests.ReactionTest
{
    [TestFixture]
    public class ListReactionTests
    {
        //private Mock<IReactionService> _reactionServiceMock;
        //private Mock<IAuthenticationService> _authenServiceMock;
        //private ReactionController _controller;

        //[SetUp]
        //public void Setup()
        //{
        //    _reactionServiceMock = new Mock<IReactionService>();
        //    _authenServiceMock = new Mock<IAuthenticationService>();
        //    _controller = new ReactionController(_reactionServiceMock.Object, _authenServiceMock.Object);
        //}

        //[Test]
        //public async Task GetAllReactionsByPost_Authenticated_ValidPostId_ReturnsSuccess()
        //{
        //    // Arrange
        //    var postId = "686d08d5e670c5b7707536d6";
        //    var expectedResponse = new ListReactionResponseDTO
        //    {
        //        Success = true,
        //        Message = "Get list of reactions successfully!",
        //        AvailableCount = 1,
        //        ReactionDTOs = new List<ReactionDTO>
        //        {
        //            new ReactionDTO
        //            {
        //                Reaction = new Reaction
        //                {
        //                    ReactionId = "1",
        //                    EntityId = postId,
        //                    EntityType = "Post",
        //                    AccId = "acc01",
        //                    CategoryReactionId = "671b9f3b7f8a9c4d3e2b1c01"
        //                },
        //                Account = new MyProfileDTO { AccId = "acc01" },
        //                CategoryReaction = new CategoryReaction { CategoryReactionId = "671b9f3b7f8a9c4d3e2b1c01" }
        //            }
        //        }
        //    };

        //    _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
        //    _reactionServiceMock.Setup(s => s.GetAllByEntityAsync(postId, "Post")).ReturnsAsync(expectedResponse);

        //    // Act
        //    var result = await _controller.GetAllReactionsByPost(postId);

        //    // Assert
        //    Assert.IsInstanceOf<OkObjectResult>(result);
        //    var okResult = result as OkObjectResult;
        //    Assert.IsNotNull(okResult);
        //    Assert.AreEqual(200, okResult.StatusCode);
        //    var response = okResult.Value as ListReactionResponseDTO;
        //    Assert.IsTrue(response.Success);
        //    Assert.AreEqual("Get list of reactions successfully!", response.Message);
        //    Assert.AreEqual(1, response.AvailableCount);
        //    Assert.IsNotNull(response.ReactionDTOs);
        //}

        //[Test]
        //public async Task GetAllReactionsByPost_Unauthenticated_ValidPostId_ReturnsSuccess()
        //{
        //    // Arrange
        //    var postId = "686d08d5e670c5b7707536d6";
        //    var expectedResponse = new ListReactionResponseDTO
        //    {
        //        Success = true,
        //        Message = "Get list of reactions successfully!",
        //        AvailableCount = 1,
        //        ReactionDTOs = new List<ReactionDTO>
        //        {
        //            new ReactionDTO
        //            {
        //                Reaction = new Reaction
        //                {
        //                    ReactionId = "1",
        //                    EntityId = postId,
        //                    EntityType = "Post",
        //                    AccId = "acc01",
        //                    CategoryReactionId = "671b9f3b7f8a9c4d3e2b1c01"
        //                },
        //                Account = new MyProfileDTO { AccId = "acc01" },
        //                CategoryReaction = new CategoryReaction { CategoryReactionId = "671b9f3b7f8a9c4d3e2b1c01" }
        //            }
        //        }
        //    };

        //    _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);
        //    _reactionServiceMock.Setup(s => s.GetAllByEntityAsync(postId, "Post")).ReturnsAsync(expectedResponse);

        //    // Act
        //    var result = await _controller.GetAllReactionsByPost(postId);

        //    // Assert
        //    Assert.IsInstanceOf<OkObjectResult>(result);
        //    var okResult = result as OkObjectResult;
        //    Assert.IsNotNull(okResult);
        //    Assert.AreEqual(200, okResult.StatusCode);
        //    var response = okResult.Value as ListReactionResponseDTO;
        //    Assert.IsTrue(response.Success);
        //    Assert.AreEqual("Get list of reactions successfully!", response.Message);
        //    Assert.AreEqual(1, response.AvailableCount);
        //    Assert.IsNotNull(response.ReactionDTOs);
        //}

        //[Test]
        //public async Task GetAllReactionsByPost_InvalidPostId_ReturnsError()
        //{
        //    // Arrange
        //    var postId = "abc";
        //    var expectedResponse = new ListReactionResponseDTO
        //    {
        //        Success = false,
        //        Message = "No reaction found!.",
        //        AvailableCount = 0,
        //        ReactionDTOs = new List<ReactionDTO>()
        //    };

        //    _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
        //    _reactionServiceMock.Setup(s => s.GetAllByEntityAsync(postId, "Post")).ReturnsAsync(expectedResponse);

        //    // Act
        //    var result = await _controller.GetAllReactionsByPost(postId);

        //    // Assert
        //    Assert.IsInstanceOf<OkObjectResult>(result);
        //    var okResult = result as OkObjectResult;
        //    Assert.IsNotNull(okResult);
        //    Assert.AreEqual(200, okResult.StatusCode);
        //    var response = okResult.Value as ListReactionResponseDTO;
        //    Assert.IsFalse(response.Success);
        //    Assert.AreEqual("No reaction found!.", response.Message);
        //    Assert.AreEqual(0, response.AvailableCount);
        //    Assert.IsNotNull(response.ReactionDTOs);
        //    Assert.IsEmpty(response.ReactionDTOs);
        //}

        //[Test]
        //public async Task GetAllReactionsByComment_Authenticated_ValidCommentId_ReturnsSuccess()
        //{
        //    // Arrange
        //    var commentId = "681627117372afdcfd2cbf3e";
        //    var expectedResponse = new ListReactionResponseDTO
        //    {
        //        Success = true,
        //        Message = "Get list of reactions successfully!",
        //        AvailableCount = 1,
        //        ReactionDTOs = new List<ReactionDTO>
        //        {
        //            new ReactionDTO
        //            {
        //                Reaction = new Reaction
        //                {
        //                    ReactionId = "1",
        //                    EntityId = commentId,
        //                    EntityType = "Comment",
        //                    AccId = "acc01",
        //                    CategoryReactionId = "671b9f3b7f8a9c4d3e2b1c02"
        //                },
        //                Account = new MyProfileDTO { AccId = "acc01" },
        //                CategoryReaction = new CategoryReaction { CategoryReactionId = "671b9f3b7f8a9c4d3e2b1c02" }
        //            }
        //        }
        //    };

        //    _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
        //    _reactionServiceMock.Setup(s => s.GetAllByEntityAsync(commentId, "Comment")).ReturnsAsync(expectedResponse);

        //    // Act
        //    var result = await _controller.GetAllReactionsByComment(commentId);

        //    // Assert
        //    Assert.IsInstanceOf<OkObjectResult>(result);
        //    var okResult = result as OkObjectResult;
        //    Assert.IsNotNull(okResult);
        //    Assert.AreEqual(200, okResult.StatusCode);
        //    var response = okResult.Value as ListReactionResponseDTO;
        //    Assert.IsTrue(response.Success);
        //    Assert.AreEqual("Get list of reactions successfully!", response.Message);
        //    Assert.AreEqual(1, response.AvailableCount);
        //    Assert.IsNotNull(response.ReactionDTOs);
        //}

        //[Test]
        //public async Task GetAllReactionsByComment_InvalidCommentId_ReturnsError()
        //{
        //    // Arrange
        //    var commentId = "681627117372afdcfd2";
        //    var expectedResponse = new ListReactionResponseDTO
        //    {
        //        Success = false,
        //        Message = "No reaction found!.",
        //        AvailableCount = 0,
        //        ReactionDTOs = new List<ReactionDTO>()
        //    };

        //    _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
        //    _reactionServiceMock.Setup(s => s.GetAllByEntityAsync(commentId, "Comment")).ReturnsAsync(expectedResponse);

        //    // Act
        //    var result = await _controller.GetAllReactionsByComment(commentId);

        //    // Assert
        //    Assert.IsInstanceOf<OkObjectResult>(result);
        //    var okResult = result as OkObjectResult;
        //    Assert.IsNotNull(okResult);
        //    Assert.AreEqual(200, okResult.StatusCode);
        //    var response = okResult.Value as ListReactionResponseDTO;
        //    Assert.IsFalse(response.Success);
        //    Assert.AreEqual("No reaction found!.", response.Message);
        //    Assert.AreEqual(0, response.AvailableCount);
        //    Assert.IsNotNull(response.ReactionDTOs);
        //    Assert.IsEmpty(response.ReactionDTOs);
        //}

        //[TearDown]
        //public void TearDown()
        //{
        //    _reactionServiceMock.Reset();
        //    _authenServiceMock.Reset();
        //}
    }
}
