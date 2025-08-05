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
using FamilyFarm.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;

namespace FamilyFarm.Tests.ProcessTest
{
    [TestFixture]
    public class CreateProcessStepTests
    {
        //private Mock<IProcessRepository> _mockProcessService;
        //private Mock<IServiceRepository> _mockServiceRepo;
        //private Mock<IProcessStepRepository> _stepRepoMock;
        //private Mock<IUploadFileService> _uploadFileMock;
        //private Mock<IAuthenticationService> _mockAuthService;
        //private Mock<IAccountRepository> _accountRepoMock;
        //private Mock<IBookingServiceRepository> _bookingServiceRepoMock;
        //private Mock<IPaymentRepository> _paymentRepoMock;
        //private ProcessController _controller;

        //[SetUp]
        //public void Setup()
        //{
        //    _mockProcessService = new Mock<IProcessRepository>();
        //    _mockServiceRepo = new Mock<IServiceRepository>();
        //    _stepRepoMock = new Mock<IProcessStepRepository>();
        //    _uploadFileMock = new Mock<IUploadFileService>();
        //    _mockAuthService = new Mock<IAuthenticationService>();
        //    _accountRepoMock = new Mock<IAccountRepository>();
        //    _bookingServiceRepoMock = new Mock<IBookingServiceRepository>();
        //    _paymentRepoMock = new Mock<IPaymentRepository>();

        //    var processService = new ProcessService(
        //        _mockProcessService.Object,
        //        _accountRepoMock.Object,
        //        _mockServiceRepo.Object,
        //        _bookingServiceRepoMock.Object,
        //        _uploadFileMock.Object,
        //        _stepRepoMock.Object,
        //        _paymentRepoMock.Object
        //    );

        //    _controller = new ProcessController(processService, _mockAuthService.Object, _uploadFileMock.Object);
        //}

        private Mock<IProcessService> _mockProcessService;  // Khai báo Mock cho IProcessService
        private Mock<IAuthenticationService> _mockAuthService;  // Khai báo Mock cho IAuthenticationService
        private Mock<IProcessRepository> _mockProcessRepo;  // Khai báo Mock cho IProcessRepository
        private Mock<IAccountRepository> _mockAccountRepo;  // Khai báo Mock cho IAccountRepository
        private Mock<IServiceRepository> _mockServiceRepo;  // Khai báo Mock cho IServiceRepository
        private Mock<IBookingServiceRepository> _mockBookingRepo;  // Khai báo Mock cho IBookingServiceRepository
        private Mock<IUploadFileService> _mockUploadFileService;  // Khai báo Mock cho IUploadFileService
        private Mock<IProcessStepRepository> _mockStepRepo;  // Khai báo Mock cho IProcessStepRepository
        private Mock<IPaymentRepository> _mockPaymentRepo;  // Khai báo Mock cho IPaymentRepository
        private ProcessController _controller;  // Controller cần test

        [SetUp]
        public void Setup()
        {
            // Khởi tạo Mock cho các service
            _mockProcessService = new Mock<IProcessService>();
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockProcessRepo = new Mock<IProcessRepository>();
            _mockAccountRepo = new Mock<IAccountRepository>();
            _mockServiceRepo = new Mock<IServiceRepository>();
            _mockBookingRepo = new Mock<IBookingServiceRepository>();
            _mockUploadFileService = new Mock<IUploadFileService>();
            _mockStepRepo = new Mock<IProcessStepRepository>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();

            // Mock các phương thức của các service
            var mapperMock = new Mock<IMapper>();  // Mock cho IMapper

            // Khởi tạo ProcessService với tất cả các mock, bao gồm mock của IMapper
            var processService = new ProcessService(
                _mockProcessRepo.Object,
                _mockAccountRepo.Object,
                _mockServiceRepo.Object,
                _mockBookingRepo.Object,
                _mockUploadFileService.Object,
                _mockStepRepo.Object,
                _mockPaymentRepo.Object,
                mapperMock.Object);  // Thêm mock IMapper vào constructor

            // Khởi tạo controller với ProcessService và các mock khác
            _controller = new ProcessController(processService, _mockAuthService.Object, _mockUploadFileService.Object);
        }

        private Service GetValidService() => new Service
        {
            ServiceId = "service123",
            CategoryServiceId = "cat001",
            ProviderId = "expert123",
            ServiceName = "Test Service",
            ServiceDescription = "Test Description",
            Price = 100000,
            Status = 1
        };

        private void SetExpertUser() =>
            _mockAuthService.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "68007b2a87b41211f0af1d57"
            });

        private ProcessRequestDTO GetValidProcessRequest() => new ProcessRequestDTO
        {
            ServiceId = "service123",
            ProcessTittle = "Chữa bệnh dạ dày",
            Description = "Mô tả quy trình chữa bệnh",
            NumberOfSteps = 1,
            ProcessSteps = new List<ProcessStepRequestDTO>
            {
                new ProcessStepRequestDTO
                {
                    StepNumber = 1,
                    StepTitle = "Uống nước ấm",
                    StepDescription = "Sáng sớm uống nước ấm",
                    Images = new List<string> { "image1.jpg" }
                }
            }
        };

        //[Test]
        //public async Task CreateProcessStep_ValidRequest_ShouldReturnSuccess()
        //{
        //    SetExpertUser();
        //    var processRequest = GetValidProcessRequest();

        //    _mockServiceRepo.Setup(x => x.GetServiceById("service123"))
        //        .ReturnsAsync(new Service
        //        {
        //            ServiceId = "service123",
        //            CategoryServiceId = "cat001",
        //            ProviderId = "expert123",
        //            ServiceName = "Test Service",
        //            ServiceDescription = "Test Description",
        //            Price = 100000,
        //            Status = 1
        //        });

        //    _mockProcessService.Setup(x => x.CreateProcess(It.IsAny<Process>()))
        //        .ReturnsAsync(new Process { ProcessId = "process123" });

        //    _mockStepRepo.Setup(x => x.CreateProcessStep(It.IsAny<ProcessStep>()))
        //        .ReturnsAsync(new ProcessStep { StepId = "step123" });

        //    _mockStepRepo.Setup(x => x.CreateStepImage(It.IsAny<ProcessStepImage>()))
        //        .Returns(Task.CompletedTask);

        //    _mockServiceRepo.Setup(x => x.UpdateProcessStatusService("service123"))
        //        .Returns(Task.CompletedTask);

        //    var result = await _controller.CreateProcess(processRequest) as OkObjectResult;

        //    Assert.IsNotNull(result);
        //    var dto = result.Value as ProcessResponseDTO;
        //    Assert.IsTrue(dto!.Success);
        //    Assert.AreEqual("Process created successfully", dto.Message);
        //}

        [Test]
        public async Task CreateProcessStep_ValidRequest_ShouldReturnSuccess()
        {
            SetExpertUser();
            var processRequest = GetValidProcessRequest();

            _mockServiceRepo.Setup(x => x.GetServiceById("service123"))
                .ReturnsAsync(new Service
                {
                    ServiceId = "service123",
                    CategoryServiceId = "cat001",
                    ProviderId = "expert123",
                    ServiceName = "Test Service",
                    ServiceDescription = "Test Description",
                    Price = 100000,
                    Status = 1
                });

            _mockProcessService.Setup(x => x.CreateProcess(It.IsAny<ProcessRequestDTO>()))
                .ReturnsAsync(new ProcessResponseDTO { Success = true, Message = "Process created successfully" });

            _mockStepRepo.Setup(x => x.CreateProcessStep(It.IsAny<ProcessStep>()))
                .ReturnsAsync(new ProcessStep { StepId = "step123" });

            _mockStepRepo.Setup(x => x.CreateStepImage(It.IsAny<ProcessStepImage>()))
                .Returns(Task.CompletedTask);

            _mockServiceRepo.Setup(x => x.UpdateProcessStatusService("service123"))
                .Returns(Task.CompletedTask);

            var result = await _controller.CreateProcess(processRequest) as OkObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsTrue(dto!.Success);
            Assert.AreEqual("Process created successfully", dto.Message);
        }


        [Test]
        public async Task CreateProcessStep_UserNotAuthenticated_ShouldReturnUnauthorized()
        {
            _mockAuthService.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.CreateProcess(GetValidProcessRequest());

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task CreateProcessStep_UserNotExpert_ShouldReturnBadRequest()
        {
            _mockAuthService.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "nonExpert"
            });

            var result = await _controller.CreateProcess(GetValidProcessRequest()) as BadRequestObjectResult;

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateProcessStep_MissingDescription_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();
            request.Description = "";

            _mockServiceRepo.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync(new Service
            {
                ServiceId = request.ServiceId,
                CategoryServiceId = "cat001",
                ProviderId = "expert123",
                ServiceName = "ServiceName",
                ServiceDescription = "ServiceDescription",
                Price = 120000,
                Status = 1
            });

            var result = await _controller.CreateProcess(request) as BadRequestObjectResult;

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateProcessStep_MissingTitle_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();
            request.ProcessTittle = "";

            _mockServiceRepo.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync(new Service
            {
                ServiceId = request.ServiceId,
                CategoryServiceId = "cat001",
                ProviderId = "expert123",
                ServiceName = "ServiceName",
                ServiceDescription = "ServiceDescription",
                Price = 120000,
                Status = 1
            });

            var result = await _controller.CreateProcess(request) as BadRequestObjectResult;

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateProcessStep_ServiceNullToLink_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();

            _mockServiceRepo.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync((Service)null);

            var result = await _controller.CreateProcess(request) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual("Service are null", dto.Message);
        }

        //[Test]
        //public async Task CreateProcessStep_WithoutImage_ShouldStillSucceed()
        //{
        //    SetExpertUser();
        //    var request = GetValidProcessRequest();
        //    request.ProcessSteps[0].Images = null;

        //    _mockServiceRepo.Setup(x => x.GetServiceById("service123")).ReturnsAsync(GetValidService());
        //    _mockProcessService.Setup(x => x.CreateProcess(It.IsAny<Process>())).ReturnsAsync(new Process { ProcessId = "p123" });
        //    _mockStepRepo.Setup(x => x.CreateProcessStep(It.IsAny<ProcessStep>())).ReturnsAsync(new ProcessStep { StepId = "s123" });
        //    _mockServiceRepo.Setup(x => x.UpdateProcessStatusService("service123")).Returns(Task.CompletedTask);

        //    var result = await _controller.CreateProcess(request) as OkObjectResult;
        //    Assert.IsNotNull(result);
        //}

        //[Test]
        //public async Task CreateProcessStep_ImageUploadFail_ShouldReturnBadRequest()
        //{
        //    SetExpertUser();
        //    var request = GetValidProcessRequest();

        //    _mockServiceRepo.Setup(x => x.GetServiceById("service123")).ReturnsAsync(GetValidService());
        //    _mockProcessService.Setup(x => x.CreateProcess(It.IsAny<Process>())).ReturnsAsync(new Process { ProcessId = "process123" });
        //    _mockStepRepo.Setup(x => x.CreateProcessStep(It.IsAny<ProcessStep>())).ReturnsAsync(new ProcessStep { StepId = "step123" });
        //    _mockStepRepo.Setup(x => x.CreateStepImage(It.IsAny<ProcessStepImage>())).ThrowsAsync(new Exception("Image upload failed"));
        //    _mockServiceRepo.Setup(x => x.UpdateProcessStatusService("service123")).Returns(Task.CompletedTask);

        //    Assert.ThrowsAsync<Exception>(async () => await _controller.CreateProcess(request));
        //}

        [Test]
        public async Task CreateProcessStep_WithoutImage_ShouldStillSucceed()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();
            request.ProcessSteps[0].Images = null;

            _mockServiceRepo.Setup(x => x.GetServiceById("service123")).ReturnsAsync(GetValidService());
            _mockProcessService.Setup(x => x.CreateProcess(It.IsAny<ProcessRequestDTO>())).ReturnsAsync(new ProcessResponseDTO { Success = true, Message = "Process created successfully" });
            _mockStepRepo.Setup(x => x.CreateProcessStep(It.IsAny<ProcessStep>())).ReturnsAsync(new ProcessStep { StepId = "s123" });
            _mockServiceRepo.Setup(x => x.UpdateProcessStatusService("service123")).Returns(Task.CompletedTask);

            var result = await _controller.CreateProcess(request) as OkObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsTrue(dto!.Success);
            Assert.AreEqual("Process created successfully", dto.Message);
        }

        [Test]
        public async Task CreateProcessStep_ImageUploadFail_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();

            _mockServiceRepo.Setup(x => x.GetServiceById("service123")).ReturnsAsync(GetValidService());
            _mockProcessService.Setup(x => x.CreateProcess(It.IsAny<ProcessRequestDTO>())).ReturnsAsync(new ProcessResponseDTO { Success = true, Message = "Process created successfully" });
            _mockStepRepo.Setup(x => x.CreateProcessStep(It.IsAny<ProcessStep>())).ReturnsAsync(new ProcessStep { StepId = "step123" });
            _mockStepRepo.Setup(x => x.CreateStepImage(It.IsAny<ProcessStepImage>())).ThrowsAsync(new Exception("Image upload failed"));
            _mockServiceRepo.Setup(x => x.UpdateProcessStatusService("service123")).Returns(Task.CompletedTask);

            Assert.ThrowsAsync<Exception>(async () => await _controller.CreateProcess(request));
        }

    }
}
