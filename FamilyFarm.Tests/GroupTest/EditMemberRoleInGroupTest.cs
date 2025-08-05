using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.GroupTest
{
    public class EditMemberRoleInGroupTest
    {
        private Mock<IGroupMemberService> _groupMemberServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private GroupMemberController _controller;
        private Mock<ISearchHistoryService> _searchHistoryServiceMock;
        private Mock<IAccountService> _accountServiceMock;

        [SetUp]
        public void Setup()
        {
            _groupMemberServiceMock = new Mock<IGroupMemberService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _searchHistoryServiceMock = new Mock<ISearchHistoryService>();
            _controller = new GroupMemberController(_groupMemberServiceMock.Object, _authServiceMock.Object,
                _searchHistoryServiceMock.Object, _accountServiceMock.Object);
        }
        [Test]
        public async Task UpdateMemberRole_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);

            // Act
            var result = await _controller.UpdateMemberRole("686668bd4a453677a54f0a79", "680cebdfac700e1cb4c165b2");

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }
        [Test]
        public async Task UpdateMemberRole_ReturnsOk_WhenUpdateSuccessful()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "688f1d98644c68a24c3a1c14" });

            _groupMemberServiceMock.Setup(x => x.UpdateMemberRoleAsync("688f1d98644c68a24c3a1c14", "680ce8722b3eec497a30201e"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateMemberRole("688f1d98644c68a24c3a1c14", "680ce8722b3eec497a30201e");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Role updated successfully.", okResult.Value);
        }
        [Test]
        public async Task UpdateMemberRole_ReturnsNotFound_WhenUpdateFails()
        {
            // Arrange
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = "6810e3831b27b2917c58d77c" });

            _groupMemberServiceMock.Setup(x => x.UpdateMemberRoleAsync("686668bd4a453677a54f0a79", "680cebdfac700e1cb4c165b2"))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateMemberRole("", "680cebdfac700e1cb4c165b2");

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Member not found or update failed.", notFoundResult.Value);
        }

    }
}
