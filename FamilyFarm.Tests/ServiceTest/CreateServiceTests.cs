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
using MongoDB.Bson;

namespace FamilyFarm.Tests.ServiceTest
{
    [TestFixture]
    public class CreateServiceTests
    {
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IUploadFileService> _uploadFileServiceMock;
        private Mock<IServiceRepository> _serviceRepoMock;
        private Mock<ICategoryServiceRepository> _categoryRepoMock;
        private Mock<IProcessRepository> _processRepoMock;
        private Mock<IProcessStepRepository> _processStepRepoMock;
        private ServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthenticationService>();
            _uploadFileServiceMock = new Mock<IUploadFileService>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _categoryRepoMock = new Mock<ICategoryServiceRepository>();
            _processRepoMock = new Mock<IProcessRepository>();
            _processStepRepoMock = new Mock<IProcessStepRepository>();

            var servicingService = new ServicingService(
                _serviceRepoMock.Object,
                _categoryRepoMock.Object,
                null,
                _uploadFileServiceMock.Object,
                _processRepoMock.Object,
                _processStepRepoMock.Object,
                null
            );

            _controller = new ServiceController(servicingService, _authServiceMock.Object);
        }

        private ServiceRequestDTO GetValidServiceRequest() => new ServiceRequestDTO
        {
            CategoryServiceId = "68127b8bad14dd90e857d389",
            ProviderId = "expert123",
            ServiceName = "Rice",
            ServiceDescription = "Good",
            Price = 120000,
            Status = 1,
            AverageRate = 0,
            RateCount = 0,
            ImageUrl = Mock.Of<IFormFile>()
        };

        private Service GetMockService() => new Service
        {
            ServiceId = "sid123",
            CategoryServiceId = "cat123",
            ProviderId = "provider123",
            ServiceName = "Test Service",
            ServiceDescription = "This is a test service.",
            Price = 100,
            Status = 1,
            AverageRate = 0,
            RateCount = 0,
            ImageUrl = "http://example.com/image.jpg",
            CreateAt = DateTime.UtcNow,
            UpdateAt = null,
            IsDeleted = false,
            HaveProcess = true
        };

        private CategoryService GetMockCategoryService() => new CategoryService
        {
            CategoryServiceId = "cat123",
            AccId = "provider123",
            CategoryName = "Mock Category",
            CategoryDescription = "Mock Description",
            CreateAt = DateTime.UtcNow,
            UpdateAt = null,
            IsDeleted = false
        };


        private UserClaimsResponseDTO GetValidExpertUser() => new UserClaimsResponseDTO
        {
            AccId = "684177674e29ef3cbb076ab5",
            RoleId = "68007b2a87b41211f0af1d57"
        };

        [Test]
        public async Task CreateService_ValidRequest_ShouldReturnSuccess()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _categoryRepoMock.Setup(x => x.GetCategoryServiceById("68127b8bad14dd90e857d389"))
                .ReturnsAsync(GetMockCategoryService());

            _uploadFileServiceMock
                .Setup(s => s.UploadImage(It.IsAny<IFormFile>()))
                .ReturnsAsync(new FileUploadResponseDTO
                {
                    UrlFile = "http://example.com/upload.jpg",
                    Message = "Upload success",
                    TypeFile = "image/jpeg",
                    CreatedAt = DateTime.UtcNow
                });


            _serviceRepoMock.Setup(x => x.CreateService(It.IsAny<Service>()))
                .ReturnsAsync(GetMockService());  // ✅ Trả về Service object

            var result = await _controller.CreateService(GetValidServiceRequest()) as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var dto = result.Value as ServiceResponseDTO;
                Assert.IsTrue(dto!.Success);
                Assert.AreEqual("Service created successfully", dto.Message);
            });
        }

        [Test]
        public async Task CreateService_MissingToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO?)null);
            var result = await _controller.CreateService(GetValidServiceRequest());
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task CreateService_NonExpertRole_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO { AccId = "686c72a8a103667c96bb6000", RoleId = "68007b0387b41211f0af1d56" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.CreateService(GetValidServiceRequest()) as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var dto = result.Value as ServiceResponseDTO;
                Assert.IsFalse(dto!.Success);
                Assert.AreEqual("Account is not expert", dto.Message);
            });
        }

        [Test]
        public async Task CreateService_CategoryNull_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.CategoryServiceId = null!;

            var result = await _controller.CreateService(request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateService_NegativePrice_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.Price = -120000;

            var result = await _controller.CreateService(request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateService_EmptyServiceName_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.ServiceName = "";

            var result = await _controller.CreateService(request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateService_EmptyDescription_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var request = GetValidServiceRequest();
            request.ServiceDescription = "";

            var result = await _controller.CreateService(request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateService_WithoutImage_ShouldStillSucceed()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _categoryRepoMock.Setup(x => x.GetCategoryServiceById(It.IsAny<string>()))
                .ReturnsAsync(GetMockCategoryService());

            _uploadFileServiceMock.Setup(x => x.UploadImage(null))
                .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "" });


            _serviceRepoMock.Setup(x => x.CreateService(It.IsAny<Service>()))
                .ReturnsAsync(new Service {
                    ServiceId = ObjectId.GenerateNewId().ToString(),
                    CategoryServiceId = "68127b8bad14dd90e857d389",
                    ProviderId = "expert123",
                    ServiceName = "Rice",
                    ServiceDescription = "Good",
                    Price = 120000,
                    Status = 1,
                    AverageRate = 0,
                    RateCount = 0,
                    ImageUrl = null
                });

            var request = GetValidServiceRequest();
            request.ImageUrl = null;

            var result = await _controller.CreateService(request) as OkObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateService_InvalidImage_ShouldFallback()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _categoryRepoMock.Setup(x => x.GetCategoryServiceById(It.IsAny<string>()))
                .ReturnsAsync(GetMockCategoryService()); // ⚠️ Phải trả về đủ field required

            _uploadFileServiceMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>()))
                .ReturnsAsync(new FileUploadResponseDTO { UrlFile = "" }); // ⚠️ Sai định dạng ảnh → fallback null

            _serviceRepoMock.Setup(x => x.CreateService(It.IsAny<Service>()))
                .ReturnsAsync(new Service
                {
                    ServiceId = "s123",
                    CategoryServiceId = "68127b8bad14dd90e857d389",
                    ProviderId = "expert123",
                    ServiceName = "Rice",
                    ServiceDescription = "Bad Image Test",
                    Price = 120000,
                    Status = 1,
                    AverageRate = 0,
                    RateCount = 0,
                    ImageUrl = null
                });

            var request = GetValidServiceRequest();
            request.ImageUrl = Mock.Of<IFormFile>(); // ⚠️ Ảnh mock nhưng coi như lỗi định dạng

            var result = await _controller.CreateService(request) as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var dto = result.Value as ServiceResponseDTO;
                Assert.IsTrue(dto!.Success);
                Assert.AreEqual("Service created successfully", dto.Message);
                Assert.That(dto.Data![0].service.ImageUrl, Is.Null.Or.Empty); // ⚠️ fallback nếu ảnh không hợp lệ
            });
        }

        [Test]
        public async Task CreateService_EmptyPrice_ShouldReturnBadRequest()
        {
            var user = GetValidExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            _categoryRepoMock.Setup(x => x.GetCategoryServiceById(It.IsAny<string>()))
                .ReturnsAsync(GetMockCategoryService()); // ⚠️ Phải trả về đủ field required

            var request = GetValidServiceRequest();
            request.Price = 0; // giả lập giá rỗng

            var result = await _controller.CreateService(request) as BadRequestObjectResult;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(result);
                var dto = result.Value as ServiceResponseDTO;
                Assert.IsFalse(dto!.Success);
                Assert.AreEqual("Price must greater than 0", dto.Message);
            });
        }

    }
}
