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
    public class SharePostTest
    {
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<ISharePostService> _sharePostServiceMock;
        private SharePostController _controller;

        [SetUp]
        public void Setup()
        {
            _authenServiceMock = new Mock<IAuthenticationService>();
            _sharePostServiceMock = new Mock<ISharePostService>();

            _controller = new SharePostController(
                _sharePostServiceMock.Object,
                _authenServiceMock.Object
            );
        }

        [Test]
        public async Task SharePost_WithValidRequest_ReturnsOk()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new SharePostRequestDTO
            {
                PostId = "686a7dfbad69ae2d37cee117",
                SharePostContent = "Nice post!",
                SharePostScope = "Public"
            };

            _sharePostServiceMock.Setup(x => x.CreateSharePost(user.AccId, request))
                .ReturnsAsync(new SharePostResponseDTO { Success = true, Message = "Shared successfully!" });

            var result = await _controller.CreateSharePost(request);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var ok = result.Result as OkObjectResult;
            Assert.IsTrue(((SharePostResponseDTO)ok!.Value!).Success!);
        }

        [Test]
        public async Task SharePost_WithoutLogin_ReturnsUnauthorized()
        {
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            var request = new SharePostRequestDTO { PostId = "686ba7d77b3b7aa52bb30857" };
            var result = await _controller.CreateSharePost(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task SharePost_WithEmptyPostId_ReturnsBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new SharePostRequestDTO { PostId = "" };

            _sharePostServiceMock.Setup(x => x.CreateSharePost(user.AccId, request))
                .ReturnsAsync((SharePostResponseDTO?)null);

            var result = await _controller.CreateSharePost(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task SharePost_WithNotFoundPostId_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new SharePostRequestDTO { PostId = "bcd" };

            _sharePostServiceMock.Setup(x => x.CreateSharePost(user.AccId, request))
                .ReturnsAsync(new SharePostResponseDTO { Success = false, Message = "Post not found" });

            var result = await _controller.CreateSharePost(request);

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task SharePost_WithPrivatePost_ReturnsNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "user123" };
            _authenServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = new SharePostRequestDTO { PostId = "686d08d5e670c5b7707536d6" };

            _sharePostServiceMock.Setup(x => x.CreateSharePost(user.AccId, request))
                .ReturnsAsync(new SharePostResponseDTO { Success = false, Message = "Cannot share private post" });

            var result = await _controller.CreateSharePost(request);

            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFound = result.Result as NotFoundObjectResult;
            Assert.AreEqual("Cannot share private post", ((SharePostResponseDTO)notFound!.Value!).Message);
        }

    }
}
