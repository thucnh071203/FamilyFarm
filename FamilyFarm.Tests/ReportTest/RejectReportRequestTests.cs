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
    public class RejectReportRequestTests
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
        public async Task Reject_AdminAuthenticated_ReportExists_ReturnsSuccess()
        {
            // Arrange
            var accId = "admin01";
            var reportId = "685d391d164b2266d37eb92a";
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
                Status = "rejected",
                CreatedAt = DateTime.UtcNow
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });
            _reportServiceMock.Setup(s => s.GetById(reportId)).ReturnsAsync(reportResponse);
            _reportServiceMock.Setup(s => s.Update(reportId, It.IsAny<Report>())).ReturnsAsync(updatedReport);

            // Act
            var result = await _controller.Reject(reportId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as Report;
            Assert.IsNotNull(response);
            Assert.AreEqual("rejected", response.Status);
        }

        [Test]
        public async Task Reject_AdminAuthenticated_InvalidReportId_ReturnsNotFound()
        {
            // Arrange
            var accId = "admin01";
            var reportId = "invalid_report_id";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId, RoleName = "Admin" });

            // Giả lập: service trả về null → report không tồn tại
            _reportServiceMock.Setup(s => s.GetById(reportId)).ReturnsAsync((ReportResponseDTO)null);

            // Act
            var result = await _controller.Reject(reportId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("Report Not Found", notFoundResult.Value);
        }


        [Test]
        public async Task Reject_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            string reportId = "some_id";
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null); // giả lập chưa đăng nhập

            // Act
            var result = await _controller.Reject(reportId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
            Assert.AreEqual("Unauthorized", unauthorizedResult.Value);
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
