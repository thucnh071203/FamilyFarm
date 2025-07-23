using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
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
    public class DeleteSearchHistoryTest
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
        public async Task DeleteSearchHistory_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var searchId = "507f191e810c19729de860ea";
            _searchHistoryService.Setup(x => x.DeleteSearchHistory(searchId))
                .ReturnsAsync(true);

            // Act
            var result = await _searchHistoryController.DeleteSearchHistory(searchId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(true, okResult.Value);
        }

        [Test]
        public async Task DeleteSearchHistory_ReturnsBadRequest_WhenFailed()
        {
            // Arrange
            var searchId = "507f191e810c19729de860ea";
            _searchHistoryService.Setup(x => x.DeleteSearchHistory(searchId))
                .ReturnsAsync(false);

            // Act
            var result = await _searchHistoryController.DeleteSearchHistory(searchId);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

    }
}
