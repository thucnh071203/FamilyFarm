using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.StatisticAdmin
{
    public class TopEngageAdmin
    {
        private StatisticsController _controller;
        private Mock<IStatisticService> _statisticServiceMock;
        private Mock<IAuthenticationService> _authMock;
        private Mock<IAccountService> _accountServiceMock;

        [SetUp]
        public void Setup()
        {
            _statisticServiceMock = new Mock<IStatisticService>();
            _authMock = new Mock<IAuthenticationService>();
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new StatisticsController(_statisticServiceMock.Object, _accountServiceMock.Object, _authMock.Object);
        }

        private List<EngagedPostResponseDTO> GetFakeEngagedPosts() => new List<EngagedPostResponseDTO>
        {
            new EngagedPostResponseDTO
            {
                Post = new Post { PostId = "6811f7c76d14cbf3be5f75a8", PostContent = "Sample Post" },
                TotalReactions = 10,
                TotalComments = 5
            }
        };

        // UTC001: top = 5, có data => trả về danh sách bài viết
        [Test]
        public async Task UTC001_GetTopEngagedPosts_WithValidTopAndData_ReturnsOk()
        {
            int top = 5;
            _statisticServiceMock.Setup(s => s.GetTopEngagedPostsAsync(top)).ReturnsAsync(GetFakeEngagedPosts());

            var result = await _controller.GetTopEngagedPosts(top);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;

            var response = ok.Value;
            Assert.IsNotNull(response);

            // Dùng reflection để kiểm tra
            var type = response.GetType();
            var isSuccessProp = type.GetProperty("isSuccess")?.GetValue(response);
            var messageProp = type.GetProperty("message")?.GetValue(response);
            var dataProp = type.GetProperty("data")?.GetValue(response);

            Assert.IsNotNull(isSuccessProp);
            Assert.AreEqual(true, isSuccessProp);
            Assert.AreEqual("Success", messageProp);
            Assert.IsNotNull(dataProp);
        }


        // UTC002: top = 5, hông có data => trả về data rỗng
        [Test]
        public async Task UTC002_GetTopEngagedPosts_WithValidTopNoData_ReturnsEmptyList()
        {
            int top = 5;
            _statisticServiceMock
                .Setup(s => s.GetTopEngagedPostsAsync(top))
                .ReturnsAsync(new List<EngagedPostResponseDTO>());

            var result = await _controller.GetTopEngagedPosts(top);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = result as OkObjectResult;

            var response = ok.Value;
            Assert.IsNotNull(response);

            var type = response.GetType();
            var isSuccessProp = type.GetProperty("isSuccess")?.GetValue(response);
            var messageProp = type.GetProperty("message")?.GetValue(response);
            var dataProp = type.GetProperty("data")?.GetValue(response);

            Assert.IsNotNull(isSuccessProp);
            Assert.AreEqual(true, isSuccessProp);
            Assert.AreEqual("Success", messageProp);
            Assert.IsNotNull(dataProp);
            Assert.AreEqual(0, ((List<EngagedPostResponseDTO>)dataProp).Count);
        }



        // UTC004: top = -1 (invalid) => trả  Badrequest
      
        [Test]
        public async Task UTC004_GetTopEngagedPosts_InvalidTopNegative_ReturnsBadRequest()
        {
            // Arrange
            int top = -1;

            // Act
            var result = await _controller.GetTopEngagedPosts(top);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = result as BadRequestObjectResult;

            // Dùng JObject để kiểm tra nội dung
            var json = JObject.FromObject(bad.Value);
            Assert.IsFalse(json.Value<bool>("isSuccess"));
            Assert.AreEqual("The value of 'top' must be greater than 0.", json.Value<string>("message"));
        }



    }
}
