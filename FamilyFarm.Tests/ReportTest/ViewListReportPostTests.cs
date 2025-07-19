using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Tests.ReportTest
{
    [TestFixture]
    public class ViewListReportPostTests
    {
        private Mock<IReportService> _reportServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private Mock<IPostService> _postServiceMock;
        private Mock<INotificationService> _notificationServiceMock;
        private ReportController _controller;

        [SetUp]
        public void Setup()
        {
            _reportServiceMock = new Mock<IReportService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _postServiceMock = new Mock<IPostService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _controller = new ReportController(
                _reportServiceMock.Object,
                _authenServiceMock.Object,
                _postServiceMock.Object,
                _notificationServiceMock.Object,
                _authenServiceMock.Object
            );
        }

        [Test]
        public async Task GetAllReport_AdminAuthenticated_ReportListHasData_ReturnsSuccessWithData()
        {
            // Arrange
            var accId = "admin01";
            var expectedResponse = new ListReportResponseDTO
            {
                Success = true,
                Message = "Get all reports successfully!",
                Data = new List<ReportDTO>
                {
                    new ReportDTO
                    {
                        Report = new Report
                        {
                            ReportId = "685d391d164b2266d37eb92a",
                            ReporterId = "reporter01",
                            PostId = "post01",
                            Reason = "Inappropriate content",
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow
                        },
                        Reporter = new MiniAccountDTO { AccId = "reporter01", FullName = "John Doe" },
                        Post = new PostMapper { Post = new Post { PostId = "post01", AccId = "user01", PostContent = "Content" } }
                    }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });
            _reportServiceMock.Setup(s => s.GetAll()).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllReport();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListReportResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get all reports successfully!", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.Count);
            Assert.AreEqual("post01", response.Data[0].Post.Post.PostId);
        }

        [Test]
        public async Task GetAllReport_AdminAuthenticated_ReportListEmpty_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var accId = "admin01";
            var expectedResponse = new ListReportResponseDTO
            {
                Success = true,
                Message = "Get all reports successfully!",
                Data = new List<ReportDTO>()
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });
            _reportServiceMock.Setup(s => s.GetAll()).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllReport();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListReportResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get all reports successfully!", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.IsEmpty(response.Data);
        }

        [Test]
        public async Task GetAllReport_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.GetAllReport();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [Test]
        public async Task GetAllReport_NonAdminAuthenticated_ReturnsForbidden()
        {
            // Arrange
            var accId = "user01";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "User" });

            // Act
            var result = await _controller.GetAllReport();

            // Assert
            Assert.IsInstanceOf<ForbidResult>(result);
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TearDown]
        public void TearDown()
        {
            _reportServiceMock.Reset();
            _authenServiceMock.Reset();
            _postServiceMock.Reset();
            _notificationServiceMock.Reset();
        }
    }
}
