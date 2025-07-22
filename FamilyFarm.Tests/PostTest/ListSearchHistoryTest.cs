using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
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
    public class ListSearchHistoryTest
    {
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<ISearchHistoryService> _searchHistoryService;
        private SearchHistoryController _searchHistoryController;
       

        [SetUp]
        public void Setup()
        {
            _authenticationService = new Mock<IAuthenticationService>();
           _searchHistoryService = new Mock<ISearchHistoryService>();

            _searchHistoryController = new SearchHistoryController
                 (_searchHistoryService.Object, _authenticationService.Object);

        }
        [Test]
        public async Task ListSearchHistory_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            _authenticationService.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _searchHistoryController.GetListSearchHistory();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        [Test]
        public async Task ListSearchHistory_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var accId = "6843e30d3c4871a0339bb1a9";
            var expectedData = new List<SearchHistory>
    {
        new SearchHistory
        {
            SearchHistoryId = "507f191e810c19729de860ea",
            AccId = accId,
            SearchKey = "fertilizer",
            SearchedAt = DateTime.UtcNow,
            IsDeleted = false
        },
        new SearchHistory
        {
            SearchHistoryId = "507f191e810c19729de860eb",
            AccId = accId,
            SearchKey = "organic farming",
            SearchedAt = DateTime.UtcNow,
            IsDeleted = false
        }
    };

            var expectedResult = new SearchHistoryResponseDTO
            {
                Success = true,
                Data = expectedData
            };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _searchHistoryService.Setup(x => x.GetListByAccId(accId))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _searchHistoryController.GetListSearchHistory();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(expectedResult, okResult.Value);
        }

        [Test]
        public async Task ListSearchHistory_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var accId = "6843e30d3c4871a0339bb1a9";
            var failedResult = new SearchHistoryResponseDTO
            {
                Success = false,
                MessageError = "Lỗi khi lấy dữ liệu"
            };

            _authenticationService.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId });

            _searchHistoryService.Setup(x => x.GetListByAccId(accId))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _searchHistoryController.GetListSearchHistory();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(failedResult, badRequestResult.Value);
        }

    }
}
