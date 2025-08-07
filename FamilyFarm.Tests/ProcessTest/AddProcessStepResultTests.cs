using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.BusinessLogic;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using FamilyFarm.Repositories;
using FamilyFarm.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FamilyFarm.API.Controllers;
using FamilyFarm.Models.Mapper;
using AutoMapper;

namespace FamilyFarm.Tests.ProcessTest
{
    [TestFixture]
    public class AddProcessStepResultTests
    {
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<IProcessRepository> _mockProcessRepo;
        private Mock<IAccountRepository> _mockAccountRepo;
        private Mock<IServiceRepository> _mockServiceRepo;
        private Mock<IBookingServiceRepository> _mockBookingRepo;
        private Mock<IUploadFileService> _mockUploadFileService;
        private Mock<IProcessStepRepository> _mockStepRepo;
        private Mock<IPaymentRepository> _mockPaymentRepo;
        private Mock<IProcessService> _mockProcessService;

        //private Mock<IAuthenticationService> _mockAuthService;
        private ProcessService _processService;
        private ProcessStepController _controller;

        [SetUp]
        //public void Setup()
        //{
        //    _mockProcessService = new Mock<IProcessService>();
        //    _mockAuthService = new Mock<IAuthenticationService>();
        //    _mockProcessRepo = new Mock<IProcessRepository>();
        //    _mockAccountRepo = new Mock<IAccountRepository>();
        //    _mockServiceRepo = new Mock<IServiceRepository>();
        //    _mockBookingRepo = new Mock<IBookingServiceRepository>();
        //    _mockUploadFileService = new Mock<IUploadFileService>();
        //    _mockStepRepo = new Mock<IProcessStepRepository>();
        //    _mockPaymentRepo = new Mock<IPaymentRepository>();
        //    //_mockAuthService = new Mock<IAuthenticationService>();



        //    _processService = new ProcessService(
        //        _mockProcessRepo.Object,
        //        _mockAccountRepo.Object,
        //        _mockServiceRepo.Object,
        //        _mockBookingRepo.Object,
        //        _mockUploadFileService.Object,
        //        _mockStepRepo.Object,
        //        _mockPaymentRepo.Object);

        //    _controller = new ProcessStepController(_mockProcessService.Object, _mockAuthService.Object);
        //}

        public void Setup()
        {
            _mockProcessService = new Mock<IProcessService>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockProcessRepo = new Mock<IProcessRepository>();
            _mockAccountRepo = new Mock<IAccountRepository>();
            _mockServiceRepo = new Mock<IServiceRepository>();
            _mockBookingRepo = new Mock<IBookingServiceRepository>();
            _mockUploadFileService = new Mock<IUploadFileService>();
            _mockStepRepo = new Mock<IProcessStepRepository>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();

            // Mock IMapper
            var mapperMock = new Mock<IMapper>();

            // Khởi tạo ProcessService với tất cả các mock, bao gồm mock của IMapper
            _processService = new ProcessService(
                _mockProcessRepo.Object,
                _mockAccountRepo.Object,
                _mockServiceRepo.Object,
                _mockBookingRepo.Object,
                _mockUploadFileService.Object,
                _mockStepRepo.Object,
                _mockPaymentRepo.Object,
                mapperMock.Object); // Thêm mock IMapper vào constructor

            _controller = new ProcessStepController(_mockProcessService.Object, _mockAuthService.Object);
        }


        private void SetFarmerUser() =>
        _mockAuthService.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
        {
            AccId = "farmer123",
            RoleId = "68007b0387b41211f0af1d56" // Farmer role
        });

        private void SetExpertUser() =>
            _mockAuthService.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "68007b2a87b41211f0af1d57"
            });

        [Test]
        public async Task TC01_ValidRequest_WithImage_ShouldReturnSuccess()
        {
            SetExpertUser();

            var request = new ProcessStepResultRequestDTO
            {
                StepId = "step123",
                StepResultComment = "Các bước chữa bệnh",
                Images = new List<IFormFile> { CreateFakeImage("test.jpg", "image/jpeg") }
            };

            _mockStepRepo.Setup(r => r.GetProcessStepResultsByStepId("step123"))
                .ReturnsAsync(new List<ProcessStepResults>
                {
                    new ProcessStepResults
                    {
                        StepId = "step123",
                        StepResultId = "existing",
                        StepResultComment = "existing",
                        CreatedAt = DateTime.UtcNow
                    }
                });

            _mockStepRepo.Setup(r => r.CreateProcessStepResult(It.IsAny<ProcessStepResults>()))
                .ReturnsAsync(new ProcessStepResults
                {
                    StepId = "step123",
                    StepResultId = "result123",
                    StepResultComment = "Các bước chữa bệnh",
                    CreatedAt = DateTime.UtcNow
                });

            _mockUploadFileService.Setup(u => u.UploadListImage(It.IsAny<List<IFormFile>>()))
                .ReturnsAsync(new List<FileUploadResponseDTO>
                {
                    new FileUploadResponseDTO { UrlFile = "http://image.jpg" }
                });

            _mockStepRepo.Setup(r => r.CreateStepResultImage(It.IsAny<StepResultImages>()))
                .ReturnsAsync(new StepResultImages
                {
                    StepResultImageId = "img123",
                    StepResultId = "result123",
                    ImageUrl = "http://image.jpg"
                });

            var result = await _processService.CreateProcessStepResult(request);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Process step result created successfully", result.Message);
            Assert.IsNotNull(result.Data);
        }

        [Test]
        public async Task TC02_NullRequest_ShouldReturnError()
        {
            var result = await _processService.CreateProcessStepResult(null);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Invalid request data", result.Message);
        }

        [Test]
        public async Task TC02_WrongRole_ShouldReturnForbid()
        {
            SetExpertUser();

            var request = new ProcessStepResultRequestDTO
            {
                StepId = "step123",
                StepResultComment = "text"
            };

            var result = await _controller.CreateProcessStepResult(request);
            Assert.IsInstanceOf<ForbidResult>(result);
        }

        [Test]
        public async Task TC03_NoStepId_ShouldReturnBadRequest()
        {
            SetFarmerUser();

            var request = new ProcessStepResultRequestDTO
            {
                StepId = null,
                StepResultComment = "valid"
            };

            _mockProcessService.Setup(s => s.CreateProcessStepResult(It.IsAny<ProcessStepResultRequestDTO>()))
                .ReturnsAsync(new ProcessStepResultResponseDTO
                {
                    Success = false, // ✅ quan trọng
                    Message = "Invalid request data",
                    Data = null
                });



            var result = await _controller.CreateProcessStepResult(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task TC04_OnlyCommentOrEmptyImage_ShouldReturnSuccess()
        {
            SetFarmerUser();

            var request = new ProcessStepResultRequestDTO
            {
                StepId = "step123",
                StepResultComment = "Chữa bệnh",
                Images = new List<IFormFile> { CreateFakeImage("empty.jpg", "image/jpeg", 0) }
            };

            _mockProcessService.Setup(s => s.CreateProcessStepResult(It.IsAny<ProcessStepResultRequestDTO>()))
                .ReturnsAsync(new ProcessStepResultResponseDTO
                {
                    Success = true,
                    Message = "Created",
                    Data = new List<ProcessStepResultMapper>() // hoặc null nếu không cần
                });


            var result = await _controller.CreateProcessStepResult(request);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task TC05_Unauthenticated_ShouldReturnUnauthorized()
        {
            _mockAuthService.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null!);

            var request = new ProcessStepResultRequestDTO
            {
                StepId = "step123",
                StepResultComment = "abc"
            };

            var result = await _controller.CreateProcessStepResult(request);
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task TC06_InvalidFileType_ShouldReturnBadRequest()
        {
            SetFarmerUser();

            var request = new ProcessStepResultRequestDTO
            {
                StepId = "step123",
                StepResultComment = "test",
                Images = new List<IFormFile> { CreateFakeImage("file.txt", "text/plain") }
            };

            _mockProcessService.Setup(s => s.CreateProcessStepResult(request))
                .ReturnsAsync(new ProcessStepResultResponseDTO
                {
                    Success = false,
                    Message = "Invalid file format",
                    Data = null // hoặc new List<StepResultDTO>() nếu bạn muốn kiểm tra chi tiết
                });


            var result = await _controller.CreateProcessStepResult(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task TC07_NoCommentAndNoImage_ShouldReturnBadRequest()
        {
            SetFarmerUser();

            var request = new ProcessStepResultRequestDTO
            {
                StepId = "step123",
                StepResultComment = "",
                Images = null
            };

            _mockProcessService.Setup(s => s.CreateProcessStepResult(request))
                .ReturnsAsync(new ProcessStepResultResponseDTO
                {
                    Success = false,
                    Message = "Result comment is required",
                    Data = null // hoặc new List<StepResultDTO>() nếu bạn muốn kiểm tra chi tiết
                });


            var result = await _controller.CreateProcessStepResult(request);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        private IFormFile CreateFakeImage(string fileName, string contentType, int length = 10)
        {
            var stream = new MemoryStream(new byte[length]);
            return new FormFile(stream, 0, length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
    }
}
