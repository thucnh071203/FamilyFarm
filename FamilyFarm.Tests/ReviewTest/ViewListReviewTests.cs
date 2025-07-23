using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
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

namespace FamilyFarm.Tests.ReviewTest
{
    [TestFixture]
    public class ViewListReviewTests
    {
        private Mock<IReviewService> _reviewServiceMock;
        private Mock<IAuthenticationService> _authenServiceMock;
        private ReviewController _controller;

        [SetUp]
        public void Setup()
        {
            _reviewServiceMock = new Mock<IReviewService>();
            _authenServiceMock = new Mock<IAuthenticationService>();
            _controller = new ReviewController(_reviewServiceMock.Object, _authenServiceMock.Object);
        }

        [Test]
        public async Task GetByServiceId_ServiceExists_HasReviews_ReturnsSuccessWithData()
        {
            // Arrange
            var serviceId = "686d084a57140dd1344df0f2";
            var expectedResponse = new ListReviewResponseDTO
            {
                Success = true,
                Message = "Get list review successfully!",
                Data = new List<ReviewDTO>
                {
                    new ReviewDTO
                    {
                        Review = new Review
                        {
                            ReviewId = "review01",
                            ServiceId = serviceId,
                            AccId = "user01",
                            Rating = 5,
                            Comment = "Excellent service",
                            CreatedAt = DateTime.UtcNow
                        },
                        Reviewer = new MyProfileDTO { AccId = "user01", FullName = "John Doe" }
                    }
                }
            };

            _reviewServiceMock.Setup(s => s.GetByServiceIdAsync(serviceId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetByServiceId(serviceId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListReviewResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get list review successfully!", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.Count);
            Assert.AreEqual(serviceId, response.Data[0].Review.ServiceId);
        }

        [Test]
        public async Task GetByServiceId_ServiceExists_NoReviews_ReturnsNotFound()
        {
            // Arrange
            var serviceId = "686d084a57140dd1344df0f2";
            var expectedResponse = new ListReviewResponseDTO
            {
                Success = false,
                Message = "No reviews found for the specified service.",
                Data = null
            };

            _reviewServiceMock.Setup(s => s.GetByServiceIdAsync(serviceId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetByServiceId(serviceId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as ListReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("No reviews found for the specified service.", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task GetByServiceId_ServiceDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var serviceId = "2837874394809182389";
            var expectedResponse = new ListReviewResponseDTO
            {
                Success = false,
                Message = "Invalid Service ID format",
                Data = null
            };

            _reviewServiceMock.Setup(s => s.GetByServiceIdAsync(serviceId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetByServiceId(serviceId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as ListReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid Service ID format", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task GetByServiceId_EmptyServiceId_ReturnsNotFound()
        {
            // Arrange
            var serviceId = "";
            var expectedResponse = new ListReviewResponseDTO
            {
                Success = false,
                Message = "Invalid Service ID format",
                Data = null
            };

            _reviewServiceMock.Setup(s => s.GetByServiceIdAsync(serviceId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetByServiceId(serviceId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var response = notFoundResult.Value as ListReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid Service ID format", response.Message);
            Assert.IsNull(response.Data);
        }

        // Note: The conditions table includes "User logged in to the system" for UTCID01, UTCID03, UTCID04, UTCID05,
        // but the controller does not check authentication. If authentication is required, add [Authorize] and test below.
        [Test]
        public async Task GetByServiceId_NotAuthenticated_ReturnsSuccess()
        {
            // Arrange
            var serviceId = "686d084a57140dd1344df0f2";
            var expectedResponse = new ListReviewResponseDTO
            {
                Success = true,
                Message = "Get list review successfully!",
                Data = new List<ReviewDTO>()
            };

            _reviewServiceMock.Setup(s => s.GetByServiceIdAsync(serviceId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetByServiceId(serviceId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ListReviewResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Get list review successfully!", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.IsEmpty(response.Data);
        }

        [TearDown]
        public void TearDown()
        {
            _reviewServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
