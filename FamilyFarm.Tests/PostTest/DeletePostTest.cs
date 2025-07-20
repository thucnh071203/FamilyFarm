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
    public class DeletePostTest
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
        public async Task SoftDelete_WithValidRequest_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _postServiceMock.Setup(x => x.TempDeleted(user.AccId, It.Is<DeletePostRequestDTO>(x => x.PostId == "684aa467e156d14823ded938")))
                .ReturnsAsync(new DeletePostResponseDTO { Success = true });

            var result = await _controller.SoftDeletedPost("684aa467e156d14823ded938");

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task SoftDelete_WithoutLogin_ReturnsNotFound()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var result = await _controller.SoftDeletedPost("684aa4abe156d14823ded93b");

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result.Result);
        }

        [Test]
        public async Task SoftDelete_WithEmptyPostId_ReturnsBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.SoftDeletedPost("");

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }


        [Test]
        public async Task SoftDelete_WithNotExistPost_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _postServiceMock.Setup(x => x.TempDeleted(user.AccId, It.Is<DeletePostRequestDTO>(x => x.PostId == "12345678929303290293")))
                .ReturnsAsync(new DeletePostResponseDTO { Success = false });

            var result = await _controller.SoftDeletedPost("12345678929303290293");

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task SoftDelete_WithOtherUserPost_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _postServiceMock.Setup(x => x.TempDeleted(user.AccId, It.Is<DeletePostRequestDTO>(x => x.PostId == "6846668fe650cf2046ad34be")))
                .ReturnsAsync(new DeletePostResponseDTO { Success = false });

            var result = await _controller.SoftDeletedPost("6846668fe650cf2046ad34be");

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task SoftDelete_WithAlreadyDeletedPost_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _postServiceMock.Setup(x => x.TempDeleted(user.AccId, It.Is<DeletePostRequestDTO>(x => x.PostId == "684aa5e74250218106250c20")))
                .ReturnsAsync(new DeletePostResponseDTO { Success = false });

            var result = await _controller.SoftDeletedPost("684aa5e74250218106250c20");

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }
    }
}
