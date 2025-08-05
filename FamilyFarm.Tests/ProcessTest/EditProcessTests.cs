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
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using AutoMapper;

namespace FamilyFarm.Tests.ProcessTest
{
    [TestFixture]
    public class EditProcessTests
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


        private void SetExpertUser() =>
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "68007b2a87b41211f0af1d57"
            });

        private ProcessUpdateRequestDTO GetValidUpdateRequest() => new ProcessUpdateRequestDTO
        {
            ServiceId = "service123",
            ProcessTittle = "Updated Title",
            Description = "Updated Description",
            NumberOfSteps = 1,
            ProcessSteps = new List<ProcessStepUpdateRequestDTO>
            {
                new ProcessStepUpdateRequestDTO
                {
                    StepId = "step1",
                    StepNumber = 1,
                    StepTitle = "Step 1",
                    StepDescription = "Desc 1",
                    ImagesWithId = new List<StepImageDTO>
                    {
                        new StepImageDTO { ProcessStepImageId = "img1", ImageUrl = "img1.jpg" },
                        new StepImageDTO { ProcessStepImageId = null, ImageUrl = "newimg.jpg" }
                    }
                }
            }
        };

        private Service GetValidService() => new Service
        {
            ServiceId = "service123",
            CategoryServiceId = "cat001",
            ProviderId = "686c72a8a103667c96bb6000",
            ServiceName = "Test",
            ServiceDescription = "Desc",
            Price = 1000,
            Status = 1
        };

        private Process GetValidProcess() => new Process
        {
            ProcessId = "process123",
            ServiceId = "service123",
            ProcessTittle = "T",
            Description = "D",
            NumberOfSteps = 1
        };

        [Test]
        public async Task UpdateProcess_ValidRequest_ShouldReturnSuccess()
        {
            SetExpertUser();
            var req = GetValidUpdateRequest();

            _serviceRepoMock.Setup(x => x.GetServiceById("service123")).ReturnsAsync(GetValidService());
            _processRepoMock.Setup(x => x.GetProcessByProcessId("process123")).ReturnsAsync(GetValidProcess());
            _processRepoMock.Setup(x => x.UpdateProcess("process123", It.IsAny<Process>())).ReturnsAsync(GetValidProcess());
            _stepRepoMock.Setup(x => x.UpdateProcessStep("step1", It.IsAny<ProcessStep>())).ReturnsAsync(new ProcessStep { StepId = "step1" });
            _stepRepoMock
            .Setup(x => x.UpdateStepImage("img1", It.IsAny<ProcessStepImage>()))
            .ReturnsAsync(new ProcessStepImage { ProcessStepImageId = "img1", ImageUrl = "img1.jpg", ProcessStepId = "step1" });

            _stepRepoMock.Setup(x => x.CreateStepImage(It.IsAny<ProcessStepImage>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateProcess("process123", req) as OkObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsTrue(dto!.Success);
            Assert.AreEqual("Process updated successfully", dto.Message);
        }

        [Test]
        public async Task UpdateProcess_MissingToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);
            var result = await _controller.UpdateProcess("process123", GetValidUpdateRequest());
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task UpdateProcess_ServiceNotFoundToLink_ShouldReturnBadRequest()
        {
            SetExpertUser();
            _serviceRepoMock.Setup(x => x.GetServiceById("service123")).ReturnsAsync((Service)null);

            var result = await _controller.UpdateProcess("process123", GetValidUpdateRequest()) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual("Service not found", dto.Message);
        }

        [Test]
        public async Task UpdateProcess_UserNotExpert_ShouldReturnBadRequest()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "nonExpert"
            });

            var result = await _controller.UpdateProcess("process123", GetValidUpdateRequest()) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task UpdateProcess_MissingTitle_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidUpdateRequest();
            request.ProcessTittle = "";

            _serviceRepoMock.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync(GetValidService());
            _processRepoMock.Setup(x => x.GetProcessByProcessId("process123")).ReturnsAsync(GetValidProcess());

            var result = await _controller.UpdateProcess("process123", request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task UpdateProcess_MissingDescription_ShouldReturnBadRequest()
        {
            SetExpertUser();
            var request = GetValidUpdateRequest();
            request.Description = "";

            _serviceRepoMock.Setup(x => x.GetServiceById(request.ServiceId)).ReturnsAsync(GetValidService());
            _processRepoMock.Setup(x => x.GetProcessByProcessId("process123")).ReturnsAsync(GetValidProcess());

            var result = await _controller.UpdateProcess("process123", request) as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }
    }
}
