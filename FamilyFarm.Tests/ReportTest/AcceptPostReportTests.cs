using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
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
    public class AcceptPostReportTests
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
        public async Task Accept_AdminAuthenticated_ReportExists_ReturnsSuccess()
        {
            // Arrange
            var accId = "admin01";
            var reportId = "685d45657b6996258699b88a";
            var reportResponse = new ReportResponseDTO
            {
                Success = true,
                Message = "Get report successfully!",
                Data = new ReportDTO
                {
                    Report = new Report
                    {
                        ReportId = reportId,
                        ReporterId = "reporter01",
                        PostId = "post01",
                        Reason = "Inappropriate content",
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    },
                    Reporter = new MiniAccountDTO { AccId = "reporter01" },
                    Post = new PostMapper { Post = new Post { PostId = "post01", AccId = "user01" } }
                }
            };
            var updatedReport = new Report
            {
                ReportId = reportId,
                ReporterId = "reporter01",
                PostId = "post01",
                Reason = "Inappropriate content",
                Status = "accepted",
                HandledById = accId,
                CreatedAt = DateTime.UtcNow
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });
            _reportServiceMock.Setup(s => s.GetById(reportId)).ReturnsAsync(reportResponse);
            _reportServiceMock.Setup(s => s.Update(reportId, It.IsAny<Report>())).ReturnsAsync(updatedReport);
            _notificationServiceMock.Setup(s => s.SendNotificationAsync(It.IsAny<SendNotificationRequestDTO>())).ReturnsAsync(new SendNotificationResponseDTO { Success = true });
            _postServiceMock.Setup(s => s.DeletePost(It.IsAny<DeletePostRequestDTO>()))
                .ReturnsAsync(new DeletePostResponseDTO
                {
                    Success = true,
                    Message = "Post deleted successfully!"
                });

            // Act
            var result = await _controller.Accept(reportId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as Report;
            Assert.IsNotNull(response);
            Assert.AreEqual("accepted", response.Status);
            Assert.AreEqual(accId, response.HandledById);
        }

        [Test]
        public async Task Accept_AdminAuthenticated_InvalidReportId_ReturnsNotFound()
        {
            // Arrange
            var accId = "admin01";
            var reportId = "invalid_report_id";
            _authenServiceMock.Setup(s => s.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });

            _reportServiceMock.Setup(s => s.GetById(reportId))
                .ReturnsAsync((ReportResponseDTO)null);

            // Act
            var result = await _controller.Accept(reportId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Cannot found the report!", notFoundResult.Value);
        }



        [Test]
        public async Task Accept_AdminAuthenticated_ReportNotFound_ReturnsNotFound()
        {
            // Arrange
            var accId = "admin01";
            var reportId = "685d45657b6996258699b88a";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });
            _reportServiceMock.Setup(s => s.GetById(reportId)).ReturnsAsync(new ReportResponseDTO { Success = false, Message = "Cannot found the report!" });

            // Act
            var result = await _controller.Accept(reportId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Cannot found the report!", notFoundResult.Value);
        }

        [Test]
        public async Task Accept_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var reportId = "685d45657b6996258699b88a";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.Accept(reportId);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
            Assert.AreEqual("Invalid token or user not found.", unauthorizedResult.Value);
        }

        [Test]
        public async Task Accept_NonAdminAuthenticated_ReturnsForbidden()
        {
            // Arrange
            var accId = "user01";
            var reportId = "685d45657b6996258699b88a";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "User" });

            // Act
            var result = await _controller.Accept(reportId);

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
