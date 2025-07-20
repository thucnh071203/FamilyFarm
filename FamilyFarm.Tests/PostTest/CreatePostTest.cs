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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.PostTest
{
    [TestFixture]
    public class CreatePostTest
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
        public async Task CreateNewPost_WithValidRequestAndImages_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { Username = "user" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var formFileMock = new Mock<IFormFile>();
            var request = new CreatePostRequestDTO
            {
                PostContent = "This is valid content",
                ListCategoryOfPost = new List<string> { "Plants", "Disease" },
                ListTagFriend = new List<string> { "68007b0387b41211f0af1d56" },
                ListImage = new List<IFormFile> { formFileMock.Object },
                Privacy = "Public"
            };

            _postServiceMock.Setup(x => x.AddPost(user.Username, request))
                .ReturnsAsync(new PostResponseDTO { Success = true, Message = "Create post is successfully." });

            var result = await _controller.CreateNewPost(request);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task CreateNewPost_WithEmptyInput_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "user" });

            var request = new CreatePostRequestDTO();

            _postServiceMock.Setup(x => x.AddPost("user", request))
                .ReturnsAsync((PostResponseDTO?)null);

            var result = await _controller.CreateNewPost(request);

            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task CreateNewPost_WithTooLongContent_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "user" });

            var request = new CreatePostRequestDTO
            {
                PostContent = new string('a', 5000) // Quá dài
            };

            _postServiceMock.Setup(x => x.AddPost("user", request))
                .ReturnsAsync((PostResponseDTO?)null);

            var result = await _controller.CreateNewPost(request);

            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task CreateNewPost_WithoutLogin_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns((UserClaimsResponseDTO?)null);

            var request = new CreatePostRequestDTO { PostContent = "This is valid content" };

            var result = await _controller.CreateNewPost(request);

            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task CreateNewPost_WithoutImages_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { Username = "user" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new CreatePostRequestDTO
            {
                PostContent = "This is valid content",
                ListCategoryOfPost = new List<string> {  }
            };

            _postServiceMock.Setup(x => x.AddPost(user.Username, request))
                .ReturnsAsync(new PostResponseDTO { Success = true });

            var result = await _controller.CreateNewPost(request);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }
    }
}
