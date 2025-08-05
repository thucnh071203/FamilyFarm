using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
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
    public class ListPostAITest
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
        public async Task GetListPostCheckedAI_ReturnsOk_WhenResultIsSuccessful()
        {
            // Arrange
            var mockUser = new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" };

            _authenServiceMock
                .Setup(x => x.GetDataFromToken())
                .Returns(mockUser);

            var mockResponse = new ListPostResponseDTO
            {
                Success = true,
                Message = "Success",
                Data = new List<PostMapper>(),
                Count = 0,
                HasMore = false
            };

            _postServiceMock.Setup(s => s.GetListPostCheckedAI()).ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.GetListPostCheckedAI();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(mockResponse, okResult.Value);
        }
        [Test]
        public async Task GetListPostCheckedAI_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            _authenServiceMock.Setup(x => x.GetDataFromToken())
                   .Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.GetListPostCheckedAI();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result.Result);
        }
        [Test]
        public async Task GetListPostCheckedAI_ReturnsBadRequest_WhenResultFails()
        {
            // Arrange
            var mockUser = new UserClaimsResponseDTO { AccId = "685660321fc7aebe254c4be1" };

            _authenServiceMock
                .Setup(x => x.GetDataFromToken())
                .Returns(mockUser);

            var mockResponse = new ListPostResponseDTO
            {
                Success = false,
                Message = "Error"
            };

            _postServiceMock.Setup(s => s.GetListPostCheckedAI()).ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.GetListPostCheckedAI();

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(mockResponse, badRequestResult.Value);
        }

    }
}
