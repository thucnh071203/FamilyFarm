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

namespace FamilyFarm.Tests.PostTest
{
    public class ModerateionPostTest
    {
        private Mock<IPostService> _postServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<ISearchHistoryService> _searchHistoryServiceMock;
        private Mock<ICohereService> _cohereServiceMock;
        private Mock<ISavedPostService> _savedPostServiceMock;
        private PostController _controller;

        [SetUp]
        public void Setup()
        {
            _postServiceMock = new Mock<IPostService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _searchHistoryServiceMock = new Mock<ISearchHistoryService>();
            _cohereServiceMock = new Mock<ICohereService>();
            _savedPostServiceMock = new Mock<ISavedPostService>();

            _controller = new PostController(
                _postServiceMock.Object,
                _authenServiceMock.Object,
                _searchHistoryServiceMock.Object,
                _savedPostServiceMock.Object,
                _cohereServiceMock.Object
            );
        }
        [Test]
        public async Task ModerationPostByAI_ReturnsOk_WhenPostIsCheckedSuccessfully()
        {
            // Arrange
            var postId = "680cebdfac700e1cb4c165bb";
            var accId = "685660321fc7aebe254c4be1";

            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _postServiceMock.Setup(x => x.CheckPostByAI(postId))
                .ReturnsAsync(true); // giả sử bài viết được kiểm duyệt thành công

            // Act
            var result = await _controller.ModerationPostByAI(postId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(true, okResult.Value);
        }
        [Test]
        public async Task ModerationPostByAI_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.ModerationPostByAI("680cebdfac700e1cb4c165bb");

            // Assert
            var unauthorized = result.Result as UnauthorizedResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual(401, unauthorized.StatusCode);
        }
        [Test]
        public async Task ModerationPostByAI_ReturnsBadRequest_WhenResultIsNull()
        {
            // Arrange
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" });

            _postServiceMock.Setup(x => x.CheckPostByAI("680cebdfac700e1cb4c165bb"))
                .ReturnsAsync((bool?)null);

            // Act
            var result = await _controller.ModerationPostByAI("680cebdfac700e1cb4c165bb");

            // Assert
            var badRequest = result.Result as BadRequestResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

    }
}
