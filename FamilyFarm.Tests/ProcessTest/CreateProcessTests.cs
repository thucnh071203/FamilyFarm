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
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using FamilyFarm.Repositories;
using AutoMapper;

namespace FamilyFarm.Tests.ProcessTest
{
    [TestFixture]
    public class CreateProcessTests
    {
        private Mock<IProcessRepository> _processRepoMock;
        private Mock<IServiceRepository> _serviceRepoMock;
        private Mock<IProcessStepRepository> _stepRepoMock;
        private Mock<IUploadFileService> _uploadFileMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IAccountRepository> _accountRepoMock;
        private Mock<IBookingServiceRepository> _bookingServiceRepoMock;
        private Mock<IPaymentRepository> _paymentRepoMock;
        private ProcessController _controller;

        [SetUp]
        //public void Setup()
        //{
        //    _processRepoMock = new Mock<IProcessRepository>();
        //    _serviceRepoMock = new Mock<IServiceRepository>();
        //    _stepRepoMock = new Mock<IProcessStepRepository>();
        //    _uploadFileMock = new Mock<IUploadFileService>();
        //    _authServiceMock = new Mock<IAuthenticationService>();
        //    _accountRepoMock = new Mock<IAccountRepository>();
        //    _bookingServiceRepoMock = new Mock<IBookingServiceRepository>();
        //    _paymentRepoMock = new Mock<IPaymentRepository>();

        //    var processService = new ProcessService(
        //        _processRepoMock.Object,
        //        _accountRepoMock.Object,
        //        _serviceRepoMock.Object,
        //        _bookingServiceRepoMock.Object,
        //        _uploadFileMock.Object,
        //        _stepRepoMock.Object,
        //        _paymentRepoMock.Object
        //    );

        //    _controller = new ProcessController(processService, _authServiceMock.Object, _uploadFileMock.Object);
        //}

        public void Setup()
        {
            _processRepoMock = new Mock<IProcessRepository>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _stepRepoMock = new Mock<IProcessStepRepository>();
            _uploadFileMock = new Mock<IUploadFileService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _bookingServiceRepoMock = new Mock<IBookingServiceRepository>();
            _paymentRepoMock = new Mock<IPaymentRepository>();

            // Mock cho IMapper
            var mapperMock = new Mock<IMapper>();

            // Khởi tạo ProcessService với tất cả các mock, bao gồm mock của IMapper
            var processService = new ProcessService(
                _processRepoMock.Object,
                _accountRepoMock.Object,
                _serviceRepoMock.Object,
                _bookingServiceRepoMock.Object,
                _uploadFileMock.Object,
                _stepRepoMock.Object,
                _paymentRepoMock.Object,
                mapperMock.Object);  // Thêm mock IMapper vào constructor

            _controller = new ProcessController(processService, _authServiceMock.Object, _uploadFileMock.Object);
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
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
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

        [Test]
        public async Task CreateProcess_ValidRequest_ShouldReturnSuccess()
        {
            SetExpertUser();
            var processRequest = GetValidProcessRequest();

            _serviceRepoMock.Setup(x => x.GetServiceById("service123"))
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

            _processRepoMock.Setup(x => x.CreateProcess(It.IsAny<Process>()))
                .ReturnsAsync(new Process { ProcessId = "process123" });

            _stepRepoMock.Setup(x => x.CreateProcessStep(It.IsAny<ProcessStep>()))
                .ReturnsAsync(new ProcessStep { StepId = "step123" });

            _stepRepoMock.Setup(x => x.CreateStepImage(It.IsAny<ProcessStepImage>()))
                .Returns(Task.CompletedTask);

            _serviceRepoMock.Setup(x => x.UpdateProcessStatusService("service123"))
                .Returns(Task.CompletedTask);

            var result = await _controller.CreateProcess(processRequest) as OkObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsTrue(dto!.Success);
            Assert.AreEqual("Process created successfully", dto.Message);
        }

        [Test]
        public async Task CreateProcess_UserNotAuthenticated_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.CreateProcess(GetValidProcessRequest());

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task CreateProcess_UserNotExpert_ShouldReturnBadRequest()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "nonExpert"
            });

            var result = await _controller.CreateProcess(GetValidProcessRequest()) as BadRequestObjectResult;

            Assert.IsNotNull(result);
        }

        [Test]
        public async Task CreateProcess_MissingDescription_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();
            request.Description = "";

            _serviceRepoMock.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync(new Service
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
        public async Task CreateProcess_MissingTitle_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();
            request.ProcessTittle = "";

            _serviceRepoMock.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync(new Service
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
        public async Task CreateProcess_ServiceNullToLink_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidProcessRequest();

            _serviceRepoMock.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync((Service)null);

            var result = await _controller.CreateProcess(request) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual("Service are null", dto.Message);
        }
    }
}
