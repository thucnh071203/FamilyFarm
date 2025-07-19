using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
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
    [TestFixture]
    public class ReportPostTests
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
        public async Task CreateReport_Authenticated_ValidPostId_ValidReason_NoExistingReport_ReturnsSuccess()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = "685d3905164b2266d37eb8f5",
                Reason = "Contains offensive content"
            };
            var expectedResponse = new ReportResponseDTO
            {
                Success = true,
                Message = "The report was created successfully.",
                Data = new ReportDTO
                {
                    Report = new Report
                    {
                        ReportId = "report1",
                        PostId = request.PostId,
                        ReporterId = "acc01",
                        Reason = request.Reason,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    Reporter = new MiniAccountDTO { AccId = "acc01" }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reportServiceMock.Setup(s => s.GetByPostAndReporter(request.PostId, "acc01")).ReturnsAsync((Report)null);
            _reportServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateReportRequestDTO>(), "acc01")).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("The report was created successfully.", response.Message);
            Assert.IsNotNull(response.Data);
        }

        [Test]
        public async Task CreateReport_Authenticated_ValidPostId2_ValidReason_NoExistingReport_ReturnsSuccess()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = "685d3905164b2266d37eb8f6",
                Reason = "Contains offensive content"
            };
            var expectedResponse = new ReportResponseDTO
            {
                Success = true,
                Message = "The report was created successfully.",
                Data = new ReportDTO
                {
                    Report = new Report
                    {
                        ReportId = "report2",
                        PostId = request.PostId,
                        ReporterId = "acc01",
                        Reason = request.Reason,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    Reporter = new MiniAccountDTO { AccId = "acc01" }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reportServiceMock.Setup(s => s.GetByPostAndReporter(request.PostId, "acc01")).ReturnsAsync((Report)null);
            _reportServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateReportRequestDTO>(), "acc01")).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("The report was created successfully.", response.Message);
            Assert.IsNotNull(response.Data);
        }

        [Test]
        public async Task CreateReport_Authenticated_InvalidPostId_ValidReason_NoExistingReport_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = "abc",
                Reason = "Contains offensive content"
            };
            var errorResponse = new ReportResponseDTO
            {
                Success = false,
                Message = "Invalid PostId.",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reportServiceMock.Setup(s => s.GetByPostAndReporter(request.PostId, "acc01")).ReturnsAsync((Report)null);
            _reportServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateReportRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid PostId.", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task CreateReport_Authenticated_NullPostId_ValidReason_NoExistingReport_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = null,
                Reason = "Contains offensive content"
            };
            var errorResponse = new ReportResponseDTO
            {
                Success = false,
                Message = "Invalid input data.",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reportServiceMock.Setup(s => s.GetByPostAndReporter(null, "acc01")).ReturnsAsync((Report)null);
            _reportServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateReportRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid input data.", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task CreateReport_Authenticated_ValidPostId_EmptyReason_NoExistingReport_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = "685d3905164b2266d37eb8f5",
                Reason = ""
            };
            var errorResponse = new ReportResponseDTO
            {
                Success = false,
                Message = "Invalid input data.",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reportServiceMock.Setup(s => s.GetByPostAndReporter(request.PostId, "acc01")).ReturnsAsync((Report)null);
            _reportServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateReportRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid input data.", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task CreateReport_Authenticated_InvalidPostId_EmptyReason_NoExistingReport_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = "abc",
                Reason = ""
            };
            var errorResponse = new ReportResponseDTO
            {
                Success = false,
                Message = "Invalid input data.",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reportServiceMock.Setup(s => s.GetByPostAndReporter(request.PostId, "acc01")).ReturnsAsync((Report)null);
            _reportServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateReportRequestDTO>(), "acc01")).ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Create(request) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid input data.", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task CreateReport_Authenticated_ValidPostId_ValidReason_ExistingReport_ReturnsConflict()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = "685d3905164b2266d37eb8f5",
                Reason = "Contains offensive content"
            };
            var existingReport = new Report
            {
                ReportId = "report1",
                PostId = request.PostId,
                ReporterId = "acc01",
                Reason = request.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = "acc01" });
            _reportServiceMock.Setup(s => s.GetByPostAndReporter(request.PostId, "acc01")).ReturnsAsync(existingReport);

            // Act
            var result = await _controller.Create(request) as ConflictObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(409, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("You have already reported this post.", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task CreateReport_NotAuthenticated_InvalidPostId_EmptyReason_NoExistingReport_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreateReportRequestDTO
            {
                PostId = "abc",
                Reason = ""
            };
            var errorResponse = new ReportResponseDTO
            {
                Success = false,
                Message = "User is not authenticated.",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.Create(request) as UnauthorizedObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);
            var response = result.Value as ReportResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("User is not authenticated.", response.Message);
            Assert.IsNull(response.Data);
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
