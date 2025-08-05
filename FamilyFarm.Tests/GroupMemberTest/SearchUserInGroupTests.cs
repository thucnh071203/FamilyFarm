using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.GroupMemberTest
{
    [TestFixture]
    public class SearchUserInGroupTests
    {
        private Mock<IGroupMemberService> _groupMemberServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<ISearchHistoryService> _searchHistoryServiceMock;
        private GroupMemberController _controller;

        [SetUp]
        public void Setup()
        {
            _groupMemberServiceMock = new Mock<IGroupMemberService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _searchHistoryServiceMock = new Mock<ISearchHistoryService>();
            _controller = new GroupMemberController(_groupMemberServiceMock.Object, _authServiceMock.Object, _searchHistoryServiceMock.Object, null!);
        }

        private List<Account> GetMockAccounts() => new()
    {
        new Account { AccId = "6808484b0849665c281db8b9", FullName = "Tran Phuong Nam", Email = "nam@example.com" },
        new Account { AccId = "68007b0387b41211f0af1d56", FullName = "Tran Phu", Email = "phu@example.com" }
    };

        //  Search exact name "Tran Phuong Nam"
        [Test]
        public async Task SearchExactMatch_ShouldReturnMatchedUser()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808482a0849665c281db8b8" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x => x.SearchUsersInGroupAsync("gid123", "Tran Phuong Nam")).ReturnsAsync(GetMockAccounts().Where(a => a.FullName == "Tran Phuong Nam").ToList());
            _searchHistoryServiceMock.Setup(x => x.AddSearchHistory("6808484b0849665c281db8b9", "Tran Phuong Nam")).ReturnsAsync(true);

            var result = await _controller.SearchUsersInGroup("gid123", "Tran Phuong Nam") as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var data = result!.Value as List<Account>;
                Assert.AreEqual(1, data?.Count);
                Assert.AreEqual("Tran Phuong Nam", data?[0].FullName);
            });
        }

        // Search with partial match "Tran Phu"
        [Test]
        public async Task SearchPartialName_ShouldReturnMatchedUser()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808482a0849665c281db8b8" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x => x.SearchUsersInGroupAsync("gid123", "Tran Phu")).ReturnsAsync(GetMockAccounts().Where(a => a.FullName.Contains("Tran Phu")).ToList());
            _searchHistoryServiceMock.Setup(x => x.AddSearchHistory("68007b0387b41211f0af1d56", "Tran Phu")).ReturnsAsync(true);

            var result = await _controller.SearchUsersInGroup("gid123", "Tran Phu") as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var data = result!.Value as List<Account>;
                Assert.AreEqual(2, data?.Count);
            });
        }

        // Search with empty keyword
        [Test]
        public async Task EmptyKeyword_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808482a0849665c281db8b8" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.SearchUsersInGroup("gid123", "") as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result!.StatusCode);
                Assert.AreEqual("Keyword is required.", result!.Value);
            });
        }

        // No token provided
        [Test]
        public async Task NoToken_ShouldReturnOkIfKeywordProvided()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);
            _groupMemberServiceMock.Setup(x => x.SearchUsersInGroupAsync("gid123", "Tran Phu")).ReturnsAsync(new List<Account>());
            _searchHistoryServiceMock.Setup(x => x.AddSearchHistory(null, "Tran Phu")).ReturnsAsync(true);

            var result = await _controller.SearchUsersInGroup("gid123", "Tran Phu") as NotFoundObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(404, result!.StatusCode);
                Assert.AreEqual("Not found members.", result!.Value);
            });
        }

        // ✅ TC05: No members found
        [Test]
        public async Task SearchNotFound_ShouldReturnNotFound()
        {
            var user = new UserClaimsResponseDTO { AccId = "6808482a0849665c281db8b8" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _groupMemberServiceMock.Setup(x => x.SearchUsersInGroupAsync("gid123", "Unknown")).ReturnsAsync(new List<Account>());
            _searchHistoryServiceMock.Setup(x => x.AddSearchHistory("acc001", "Unknown")).ReturnsAsync(true);

            var result = await _controller.SearchUsersInGroup("gid123", "Unknown") as NotFoundObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(404, result!.StatusCode);
                Assert.AreEqual("Not found members.", result!.Value);
            });
        }

        // ✅ TC06: Keyword with only spaces
        [Test]
        public async Task KeywordWithSpaces_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "acc001" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.SearchUsersInGroup("gid123", "   ") as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(400, result!.StatusCode);
                Assert.AreEqual("Keyword is required.", result!.Value);
            });
        }
    }
}
