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
    public class UpdatePostTest
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
        public async Task UpdatePost_WithValidRequestFullInput_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { Username = "user" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var formFileMock = new Mock<IFormFile>();
            var request = new UpdatePostRequestDTO
            {
                PostId = "post123",
                Content = "This is valid content",
                Privacy = "Public",
                IsDeleteAllImage = false,
                ImagesToAdd = new List<IFormFile> { formFileMock.Object },
                CategoriesToAdd = new List<string> { "Disease", "Plants" },
                HashTagToAdd = new List<string> { "68007b0387b41211f0af1d56" }
            };

            _postServiceMock.Setup(x => x.UpdatePost(user.Username, request))
                .ReturnsAsync(new PostResponseDTO { Success = true });

            var result = await _controller.UpdatePost(request);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdatePost_WithEmptyRequest_ReturnsBadRequest()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { Username = "user" });

            var result = await _controller.UpdatePost(null);
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdatePost_WithTooLongContent_ReturnsBadRequest()
        {
            var user = new UserClaimsResponseDTO { Username = "user" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new UpdatePostRequestDTO
            {
                PostId = "post123",
                Content = new string('a', 5000)
            };

            _postServiceMock.Setup(x => x.UpdatePost(user.Username, request))
                .ReturnsAsync((PostResponseDTO?)null);

            var result = await _controller.UpdatePost(request);
            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task UpdatePost_WithoutLogin_ReturnsNotFound()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var request = new UpdatePostRequestDTO { PostId = "post123" };
            var result = await _controller.UpdatePost(request);

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdatePost_WithSameOldContent_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { Username = "user" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new UpdatePostRequestDTO
            {
                PostId = "post123",
                Content = "old content",
                Privacy = "Public"
            };

            _postServiceMock.Setup(x => x.UpdatePost(user.Username, request))
                .ReturnsAsync(new PostResponseDTO { Success = true });

            var result = await _controller.UpdatePost(request);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdatePost_WithNoImagesAndEmptyCategory_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { Username = "user" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new UpdatePostRequestDTO
            {
                PostId = "post123",
                Content = "This is valid content",
                Privacy = "Private",
                ImagesToAdd = null,
                CategoriesToAdd = new List<string>()
            };

            _postServiceMock.Setup(x => x.UpdatePost(user.Username, request))
                .ReturnsAsync(new PostResponseDTO { Success = true });

            var result = await _controller.UpdatePost(request);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }
    }
}
