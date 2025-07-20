using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.PostTest
{
    [TestFixture]
    public class SavePostTest
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
        public async Task SavePost_WithValidRequest_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.SavedPost(user.AccId, "684aa5e74250218106250c20"))
                .ReturnsAsync(new CreatedSavedPostResponseDTO { Success = true });

            var result = await _controller.SavedPost("684aa5e74250218106250c20");

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsTrue(((CreatedSavedPostResponseDTO)okResult!.Value!).Success!);
        }

        [Test]
        public async Task SavePost_WithoutLogin_ReturnsUnauthorized()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.SavedPost("684aa4abe156d14823ded93b");

            Assert.IsInstanceOf<UnauthorizedResult>(result.Result);
        }

        [Test]
        public async Task SavePost_WithEmptyPostId_ReturnsBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.SavedPost(user.AccId, null))
                .ReturnsAsync((CreatedSavedPostResponseDTO?)null);

            var result = await _controller.SavedPost(null);

            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task SavePost_PostIdNotExist_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.SavedPost(user.AccId, "notExistPostId"))
                .ReturnsAsync(new CreatedSavedPostResponseDTO { Success = false, Message = "Post not found" });

            var result = await _controller.SavedPost("notExistPostId");

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task SavePost_AlreadySaved_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.SavedPost(user.AccId, "alreadySavedPost"))
                .ReturnsAsync(new CreatedSavedPostResponseDTO { Success = false, Message = "Already saved" });

            var result = await _controller.SavedPost("alreadySavedPost");

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFound = result.Result as NotFoundObjectResult;
            Assert.AreEqual("Already saved", ((CreatedSavedPostResponseDTO)notFound!.Value!).Message);
        }

        [Test]
        public async Task SavePost_SavingOwnPost_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _savedPostServiceMock.Setup(x => x.SavedPost(user.AccId, "ownPostId"))
                .ReturnsAsync(new CreatedSavedPostResponseDTO { Success = false, Message = "Cannot save own post" });

            var result = await _controller.SavedPost("ownPostId");

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFound = result.Result as NotFoundObjectResult;
            Assert.AreEqual("Cannot save own post", ((CreatedSavedPostResponseDTO)notFound!.Value!).Message);
        }
    }
}
