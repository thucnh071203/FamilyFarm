using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.BusinessLogic;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.ServiceTest
{
    [TestFixture]
    public class EditServiceTests
    {
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IUploadFileService> _uploadFileServiceMock;
        private Mock<IServiceRepository> _serviceRepoMock;
        private Mock<ICategoryServiceRepository> _categoryRepoMock;
        private ServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthenticationService>();
            _uploadFileServiceMock = new Mock<IUploadFileService>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _categoryRepoMock = new Mock<ICategoryServiceRepository>();

            var servicingService = new ServicingService(
                _serviceRepoMock.Object,
                _categoryRepoMock.Object,
                null,
                _uploadFileServiceMock.Object,
                null,
                null,
                null
            );

            _controller = new ServiceController(servicingService, _authServiceMock.Object);
        }

        private ServiceRequestDTO GetValidServiceRequest() => new ServiceRequestDTO
        {
            CategoryServiceId = "68127b8bad14dd90e857d389",
            ProviderId = "686c72a8a103667c96bb6000",
            ServiceName = "Rice",
            ServiceDescription = "Good",
            Price = 120000,
            Status = 1,
            AverageRate = 0,
            RateCount = 0,
            ImageUrl = Mock.Of<IFormFile>()
        };

        private Service GetServiceOwner(string name, string categoryId, string description, decimal price, int status) => new Service
        {
            ServiceId = "sid123",
            ProviderId = "686c72a8a103667c96bb6000",
            CategoryServiceId = categoryId,
            ServiceName = name,
            ServiceDescription = description,
            Price = price,
            ImageUrl = "old.jpg",
            Status = status
        };

        private CategoryService GetMockCategoryService() => new CategoryService
        {
            CategoryServiceId = "68127b8bad14dd90e857d389",
            AccId = "686c72a8a103667c96bb6000",
            CategoryName = "Agri",
            CategoryDescription = "Farm things"
        };

        private UserClaimsResponseDTO GetValidExpertUser() => new UserClaimsResponseDTO
        {
            AccId = "686c72a8a103667c96bb6000",
            RoleId = "68007b2a87b41211f0af1d57"
        };

        private Service GetUpdatedService() => new Service
        {
            ServiceId = "sid123",
            ProviderId = "686c72a8a103667c96bb6000",
            CategoryServiceId = "68127b8bad14dd90e857d389",
            ServiceName = "Rice",
            ServiceDescription = "Good",
            Price = 120000,
            ImageUrl = "old.jpg",
            Status = 1
        };

        [Test]
        public async Task EditService_ValidRequest_ShouldReturnSuccess()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _categoryRepoMock.Setup(x => x.GetCategoryServiceById(It.IsAny<string>())).ReturnsAsync(GetMockCategoryService());
            _serviceRepoMock.Setup(x => x.GetServiceById("sid123")).ReturnsAsync(GetServiceOwner("Rice", "68127b8bad14dd90e857d389", "Good", 120000, 1));
            _uploadFileServiceMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ReturnsAsync(new FileUploadResponseDTO { UrlFile = "http://image.com/image.jpg" });
            _serviceRepoMock.Setup(x => x.UpdateService("sid123", It.IsAny<Service>())).ReturnsAsync(GetUpdatedService());

            var result = await _controller.UpdateService("sid123", GetValidServiceRequest()) as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var response = result.Value as ServiceResponseDTO;
                Assert.IsNotNull(response);
                Assert.IsTrue(response.Success);
                Assert.AreEqual("Service updated successfully", response.Message);
            });
        }

        [Test]
        public async Task EditService_MissingToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);
            var result = await _controller.UpdateService("sid123", GetValidServiceRequest());
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task EditService_NotExpertRole_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "686c72a8a103667c96bb6000", RoleId = "otherRole" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.UpdateService("sid123", GetValidServiceRequest()) as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var dto = result.Value as ServiceResponseDTO;
                Assert.IsFalse(dto!.Success);
                Assert.AreEqual("Account is not expert", dto.Message);
            });
        }

        [Test]
        public async Task EditService_CategoryNull_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.CategoryServiceId = null!;

            var result = await _controller.UpdateService("sid123", request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditService_NegativePrice_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.Price = -120000;

            var result = await _controller.UpdateService("sid123", request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditService_EmptyServiceName_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.ServiceName = "";

            var result = await _controller.UpdateService("sid123", request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditService_EmptyDescription_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.ServiceDescription = "";

            var result = await _controller.UpdateService("sid123", request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditService_EmptyPrice_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.Price = 0;

            var result = await _controller.UpdateService("sid123", request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditService_WithoutImage_ShouldUseOldImage()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _categoryRepoMock.Setup(x => x.GetCategoryServiceById(It.IsAny<string>())).ReturnsAsync(GetMockCategoryService());
            _serviceRepoMock.Setup(x => x.GetServiceById("sid123")).ReturnsAsync(GetServiceOwner("Rice", "68127b8bad14dd90e857d389", "Good", 120000, 1));
            _serviceRepoMock.Setup(x => x.UpdateService("sid123", It.IsAny<Service>())).ReturnsAsync(GetServiceOwner("Rice", "68127b8bad14dd90e857d389", "Good", 120000, 1));

            var request = GetValidServiceRequest();
            request.ImageUrl = null;

            var result = await _controller.UpdateService("sid123", request) as OkObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task EditService_InvalidImageUpload_ShouldFallbackToOldImage()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _categoryRepoMock.Setup(x => x.GetCategoryServiceById(It.IsAny<string>())).ReturnsAsync(GetMockCategoryService());
            _serviceRepoMock.Setup(x => x.GetServiceById("sid123")).ReturnsAsync(GetServiceOwner("Rice", "68127b8bad14dd90e857d389", "Good", 120000, 1));
            _uploadFileServiceMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ReturnsAsync(new FileUploadResponseDTO { UrlFile = "" });
            _serviceRepoMock.Setup(x => x.UpdateService("sid123", It.IsAny<Service>())).ReturnsAsync(GetServiceOwner("Rice", "68127b8bad14dd90e857d389", "Good", 120000, 1));

            var request = GetValidServiceRequest();

            var result = await _controller.UpdateService("sid123", request) as OkObjectResult;
            Assert.IsNotNull(result);
        }
    }
}
