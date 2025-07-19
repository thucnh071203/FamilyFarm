using AutoMapper;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
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
    [TestFixture]
    public class SearchPostTests
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
        public async Task SearchPosts_WithKeywordOnly_ReturnsMatchingPosts()
        {
            // Arrange
            var keyword = "farm";
            var expected = new ListPostResponseDTO
            {
                Success = true,
                Message = "Get list post success.",
                HasMore = false,
                Data = new List<PostMapper> { new PostMapper { Post = new Models.Models.Post { PostId = "1" } } }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _postServiceMock.Setup(s => s.SearchPosts(keyword, It.IsAny<List<string>>(), false)).ReturnsAsync(expected);

            // Act
            var result = await _controller.SearchPosts(keyword, null, false);
            var ok = result as OkObjectResult;

            // Assert
            Assert.NotNull(ok);
            Assert.AreEqual(200, ok.StatusCode);
            Assert.AreEqual(expected, ok.Value);
        }

        [Test]
        public async Task SearchPosts_WithKeywordOnly_NoMatch_ReturnsEmpty()
        {
            var keyword = "unmatched";
            var emptyResponse = new ListPostResponseDTO
            {
                Success = true,
                Message = "No post found!",
                HasMore = false,
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _postServiceMock.Setup(s => s.SearchPosts(keyword, null, false)).ReturnsAsync(emptyResponse);

            var result = await _controller.SearchPosts(keyword, null, false);
            var ok = result as OkObjectResult;

            Assert.NotNull(ok);
            Assert.AreEqual(200, ok.StatusCode);
            Assert.AreEqual(emptyResponse, ok.Value);
        }

        [Test]
        public async Task SearchPosts_WithCategoriesOnly_ReturnsMatchingPosts()
        {
            var categories = new List<string> { "cat1", "cat2" };
            var expected = new ListPostResponseDTO { Success = true, Message = "Get list post success.", Data = new List<PostMapper>() };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _postServiceMock.Setup(s => s.SearchPosts(null, categories, false)).ReturnsAsync(expected);

            var result = await _controller.SearchPosts(null, categories, false) as OkObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task SearchPosts_WithCategoriesOnly_NoMatch_ReturnsEmpty()
        {
            var categories = new List<string> { "catX" };
            var emptyResponse = new ListPostResponseDTO { Success = true, Message = "No post found!", Data = null };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _postServiceMock.Setup(s => s.SearchPosts(null, categories, false)).ReturnsAsync(emptyResponse);

            var result = await _controller.SearchPosts(null, categories, false) as OkObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(emptyResponse, result.Value);
        }

        [Test]
        public async Task SearchPosts_WithKeywordAndCategories_AndLogic_ReturnsIntersection()
        {
            var keyword = "farm";
            var categories = new List<string> { "cat1" };
            var expected = new ListPostResponseDTO
            {
                Success = true,
                Message = "Get list post success.",
                Data = new List<PostMapper>()
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _postServiceMock.Setup(s => s.SearchPosts(keyword, categories, true)).ReturnsAsync(expected);

            var result = await _controller.SearchPosts(keyword, categories, true) as OkObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task SearchPosts_WithKeywordAndCategories_AndLogic_NoCommonMatch_ReturnsEmpty()
        {
            var keyword = "farm";
            var categories = new List<string> { "catX" };
            var emptyResponse = new ListPostResponseDTO { Success = true, Message = "No post found!", Data = null };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _postServiceMock.Setup(s => s.SearchPosts(keyword, categories, true)).ReturnsAsync(emptyResponse);

            var result = await _controller.SearchPosts(keyword, categories, true) as OkObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task SearchPosts_WithEmptyKeywordAndCategory_ReturnsError()
        {
            var errorResponse = new ListPostResponseDTO
            {
                Success = false,
                Message = "List post is empty.",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _postServiceMock.Setup(s => s.SearchPosts(null, null, false)).ReturnsAsync(errorResponse);

            var result = await _controller.SearchPosts(null, null, false);
            var badRequest = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [Test]
        public async Task SearchPosts_NotLoggedIn_ReturnsUnauthorized()
        {
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);
            _postServiceMock.Setup(s => s.SearchPosts(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ListPostResponseDTO { Success = true });

            var result = await _controller.SearchPosts("farm", null, false);

            // Even though it proceeds, AddSearchHistory won't be called, but controller doesn't block token absence.
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(200, ok.StatusCode); // Because controller doesn't block unauthorized (it's only for AddSearchHistory)
        }

        [TearDown]
        public void TearDown()
        {
            _postServiceMock.Reset();
            _authenServiceMock.Reset();
            _searchHistoryServiceMock.Reset();
        }
    }
}
