using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.PostTest
{
    [TestFixture]
    public class UnsavePostTest
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
        public async Task UnsavePost_WithValidRequest_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.UnsavedPost(user.AccId, "680cec6a8430d521f491db5b"))
                .ReturnsAsync(true);

            var result = await _controller.UnsavedPost("680cec6a8430d521f491db5b");

            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsTrue((bool)okResult!.Value!);
        }

        [Test]
        public async Task UnsavePost_WithoutLogin_ReturnsUnauthorized()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.UnsavedPost("684aa467e156d14823ded938");

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result.Result);
        }

        [Test]
        public async Task UnsavePost_WithNotExistPost_ReturnsOkFalse()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.UnsavedPost(user.AccId, "1234387483748737"))
                .ReturnsAsync(false);

            var result = await _controller.UnsavedPost("1234387483748737");

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsFalse((bool)okResult!.Value!);
        }

        [Test]
        public async Task UnsavePost_WithNotSavedPost_ReturnsOkFalse()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.UnsavedPost(user.AccId, "684aa3e8e156d14823ded936"))
                .ReturnsAsync(false);

            var result = await _controller.UnsavedPost("684aa3e8e156d14823ded936");

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsFalse((bool)okResult!.Value!);
        }

        [Test]
        public async Task UnsavePost_WithEmptyPostId_ReturnsOkFalse()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.UnsavedPost(user.AccId, ""))
                .ReturnsAsync(false);

            var result = await _controller.UnsavedPost("");

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsFalse((bool)okResult!.Value!);
        }

    }
}
