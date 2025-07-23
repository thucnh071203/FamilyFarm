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

namespace FamilyFarm.Tests.ReviewTest
{
    [TestFixture]
    public class AddReviewOfServiceTests
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
        public async Task Create_Authenticated_ServiceExists_ValidReview_ReturnsSuccess()
        {
            // Arrange
            var accId = "user01";
            var serviceId = "686e6c981468d16fd8becc25";
            var request = new ReviewRequestDTO
            {
                ServiceId = serviceId,
                Rating = 5,
                Comment = "Very good"
            };
            var expectedResponse = new ReviewResponseDTO
            {
                Success = true,
                Message = "Review created successfully",
                Data = new ReviewDTO
                {
                    Review = new Review
                    {
                        ReviewId = "review01",
                        ServiceId = serviceId,
                        AccId = accId,
                        Rating = 5,
                        Comment = "Very good",
                        CreatedAt = DateTime.UtcNow
                    },
                    Reviewer = new MyProfileDTO { AccId = accId, FullName = "John Doe" }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _reviewServiceMock.Setup(s => s.CreateAsync(request, accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ReviewResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Review created successfully", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(serviceId, response.Data.Review.ServiceId);
            Assert.AreEqual(5, response.Data.Review.Rating);
            Assert.AreEqual("Very good", response.Data.Review.Comment);
        }

        [Test]
        public async Task Create_NotAuthenticated_ReturnsBadRequest()
        {
            // Arrange
            var request = new ReviewRequestDTO
            {
                ServiceId = "686e6c981468d16fd8becc25",
                Rating = 5,
                Comment = "Very good"
            };
            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as CommentResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Please Login!", response.Message);
        }

        [Test]
        public async Task Create_Authenticated_ServiceExists_InvalidRatingZero_ReturnsBadRequest()
        {
            // Arrange
            var accId = "user01";
            var serviceId = "686e6c981468d16fd8becc25";
            var request = new ReviewRequestDTO
            {
                ServiceId = serviceId,
                Rating = 0,
                Comment = "Any comment"
            };
            var expectedResponse = new ReviewResponseDTO
            {
                Success = false,
                Message = "Rating must be between 1 and 5 stars",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _reviewServiceMock.Setup(s => s.CreateAsync(request, accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as ReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Rating must be between 1 and 5 stars", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task Create_Authenticated_ServiceExists_ValidRating_NoComment_ReturnsSuccess()
        {
            // Arrange
            var accId = "user01";
            var serviceId = "686e6c981468d16fd8becc25";
            var request = new ReviewRequestDTO
            {
                ServiceId = serviceId,
                Rating = 5,
                Comment = ""
            };
            var expectedResponse = new ReviewResponseDTO
            {
                Success = true,
                Message = "Review created successfully",
                Data = new ReviewDTO
                {
                    Review = new Review
                    {
                        ReviewId = "review01",
                        ServiceId = serviceId,
                        AccId = accId,
                        Rating = 5,
                        Comment = "",
                        CreatedAt = DateTime.UtcNow
                    },
                    Reviewer = new MyProfileDTO { AccId = accId }
                }
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _reviewServiceMock.Setup(s => s.CreateAsync(request, accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var response = okResult.Value as ReviewResponseDTO;
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Review created successfully", response.Message);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("", response.Data.Review.Comment);
        }

        [Test]
        public async Task Create_Authenticated_ServiceDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            var accId = "user01";
            var serviceId = "2984572039840349834";
            var request = new ReviewRequestDTO
            {
                ServiceId = serviceId,
                Rating = 5,
                Comment = "Any comment"
            };
            var expectedResponse = new ReviewResponseDTO
            {
                Success = false,
                Message = "Invalid Service ID or Account ID",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _reviewServiceMock.Setup(s => s.CreateAsync(request, accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as ReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid Service ID or Account ID", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task Create_Authenticated_EmptyServiceId_ReturnsBadRequest()
        {
            // Arrange
            var accId = "user01";
            var request = new ReviewRequestDTO
            {
                ServiceId = "",
                Rating = 5,
                Comment = "Any comment"
            };
            var expectedResponse = new ReviewResponseDTO
            {
                Success = false,
                Message = "Invalid review data",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _reviewServiceMock.Setup(s => s.CreateAsync(request, accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as ReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid review data", response.Message);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task Create_Authenticated_NullRating_ReturnsBadRequest()
        {
            // Arrange
            var accId = "user01";
            var serviceId = "686e6c981468d16fd8becc25";
            var request = new ReviewRequestDTO
            {
                ServiceId = serviceId,
                Rating = 0,
                Comment = "Any comment"
            };
            var expectedResponse = new ReviewResponseDTO
            {
                Success = false,
                Message = "Invalid review data",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _reviewServiceMock.Setup(s => s.CreateAsync(request, accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as ReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Invalid review data", response.Message);
            Assert.IsNull(response.Data);
        }

        // Note: The conditions table includes "The service exists and the user has not completed the service" (UTCID04),
        // but the ReviewService does not check this. If this check is added, include a test below.
        [Test]
        public async Task Create_Authenticated_ServiceExists_UserNotCompletedService_ReturnsBadRequest()
        {
            // Arrange
            var accId = "user01";
            var serviceId = "686e6c981468d16fd8becc25";
            var request = new ReviewRequestDTO
            {
                ServiceId = serviceId,
                Rating = 5,
                Comment = "Any comment"
            };
            var expectedResponse = new ReviewResponseDTO
            {
                Success = false,
                Message = "User has not completed the service",
                Data = null
            };

            _authenServiceMock.Setup(s => s.GetDataFromToken()).Returns(new UserClaimsResponseDTO { AccId = accId });
            _reviewServiceMock.Setup(s => s.CreateAsync(request, accId)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            var response = badRequestResult.Value as ReviewResponseDTO;
            Assert.IsFalse(response.Success);
            Assert.AreEqual("User has not completed the service", response.Message);
            Assert.IsNull(response.Data);
        }

        [TearDown]
        public void TearDown()
        {
            _reviewServiceMock.Reset();
            _authenServiceMock.Reset();
        }
    }
}
