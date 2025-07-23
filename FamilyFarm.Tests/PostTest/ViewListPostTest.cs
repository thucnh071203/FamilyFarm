using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.PostTest
{
    [TestFixture]
    public class ViewListPostTest
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
        public async Task ListPostInfinite_WithValidTokenAndValidRequest_ReturnsOk()
        {
            var expected = new ListPostResponseDTO
            {
                Success = true,
                Data = new List<PostMapper> { new PostMapper { Post = new Post { PostId = "1" } } }
            };
            _postServiceMock.Setup(x => x.GetListInfinitePost(null, 5)).ReturnsAsync(expected);

            var result = await _controller.ListPostInfinite(null, 5);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsTrue(((ListPostResponseDTO)okResult!.Value!).Success == true);
            Assert.IsNotEmpty(((ListPostResponseDTO)okResult.Value!).Data);
        }

        [Test]
        public async Task ListPostInfinite_WithInvalidPageSize_ReturnsDefaultPageSize()
        {
            var expected = new ListPostResponseDTO { Success = true, Data = new List<PostMapper>() };
            _postServiceMock.Setup(x => x.GetListInfinitePost(null, 5)).ReturnsAsync(expected);

            var result = await _controller.ListPostInfinite(null, 0);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task ListPostInfinite_WithNoPosts_ReturnsEmptyList()
        {
            var expected = new ListPostResponseDTO { Success = true, Data = new List<PostMapper>() };
            _postServiceMock.Setup(x => x.GetListInfinitePost(null, 10)).ReturnsAsync(expected);

            var result = await _controller.ListPostInfinite(null, 10);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsEmpty(((ListPostResponseDTO)okResult!.Value!).Data);
        }

        [Test]
        public async Task ListPostInfinite_WithValidRequest_ReturnsOk()
        {
            var expected = new ListPostResponseDTO
            {
                Success = true,
                Data = new List<PostMapper> { new PostMapper { Post = new Post { AccId = "1" } } }
            };
            _postServiceMock.Setup(x => x.GetListInfinitePost("abc", 10)).ReturnsAsync(expected);

            var result = await _controller.ListPostInfinite("abc", 10);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsTrue(((ListPostResponseDTO)okResult!.Value!).Success == true);
            Assert.IsNotEmpty(((ListPostResponseDTO)okResult.Value!).Data);
        }

        [Test]
        public async Task ListPostInfinite_WithNoMorePosts_ReturnsEmptyList()
        {
            var expected = new ListPostResponseDTO { Success = true, Data = new List<PostMapper>(), HasMore = false };
            _postServiceMock.Setup(x => x.GetListInfinitePost("lastid", 10)).ReturnsAsync(expected);

            var result = await _controller.ListPostInfinite("lastid", 10);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = (ListPostResponseDTO)okResult!.Value!;
            Assert.IsTrue(response.Success == true);
            Assert.IsEmpty(response.Data);
            Assert.IsFalse(response.HasMore);
        }

        [Test]
        public async Task ListPostInfinite_WithZeroPageSize_ReturnsEmptyList()
        {
            var expected = new ListPostResponseDTO { Success = true, Data = new List<PostMapper>(), HasMore = false };
            _postServiceMock.Setup(x => x.GetListInfinitePost(null, 5)).ReturnsAsync(expected);

            var result = await _controller.ListPostInfinite(null, 0);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = (ListPostResponseDTO)okResult!.Value!;
            Assert.IsTrue(response.Success == true);
            Assert.IsEmpty(response.Data);
        }

        [Test]
        public async Task ListPostInfinite_PageSizeMoreThan50_DefaultsTo5()
        {
            var expected = new ListPostResponseDTO
            {
                Success = true,
                Data = new List<PostMapper> { new PostMapper { Post = new Post { AccId = "1" } } }
            };
            _postServiceMock.Setup(x => x.GetListInfinitePost(null, 5)).ReturnsAsync(expected);

            var result = await _controller.ListPostInfinite(null, 55);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = (ListPostResponseDTO)okResult!.Value!;
            Assert.IsTrue(response.Success == true);
            Assert.IsNotEmpty(response.Data);
        }
    }
}
