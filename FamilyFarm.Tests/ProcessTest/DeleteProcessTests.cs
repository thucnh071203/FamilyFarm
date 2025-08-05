using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.ProcessTest
{
    [TestFixture]
    public class DeleteProcessTests
    {
        private Mock<IProcessRepository> _processRepoMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private ProcessController _controller;

        [SetUp]
        //public void Setup()
        //{
        //    _processRepoMock = new Mock<IProcessRepository>();
        //    _authServiceMock = new Mock<IAuthenticationService>();

        //    var service = new ProcessService(
        //        _processRepoMock.Object,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null);

        //    _controller = new ProcessController(service, _authServiceMock.Object, null);
        //}
        public void Setup()
        {
            _processRepoMock = new Mock<IProcessRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();

            // Mock IMapper
            var mapperMock = new Mock<IMapper>();

            // Khởi tạo ProcessService với tất cả các mock, bao gồm mock của IMapper
            var service = new ProcessService(
                _processRepoMock.Object,
                null, // Đối tượng IAccountRepository (có thể null trong unit test này)
                null, // Đối tượng IServiceRepository (có thể null trong unit test này)
                null, // Đối tượng IBookingServiceRepository (có thể null trong unit test này)
                null, // Đối tượng IUploadFileService (có thể null trong unit test này)
                null, // Đối tượng IProcessStepRepository (có thể null trong unit test này)
                null, // Đối tượng IPaymentRepository (có thể null trong unit test này)
                mapperMock.Object); // Thêm mock IMapper vào constructor

            _controller = new ProcessController(service, _authServiceMock.Object, null);
        }


        private void SetExpertUser() =>
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "68007b2a87b41211f0af1d57"
            });

        [Test]
        public async Task UTCID01_DeleteProcess_Valid_ShouldReturnSuccess()
        {
            SetExpertUser();
            _processRepoMock.Setup(x => x.DeleteProcess("682168c42663032aed86ff94"))
                .ReturnsAsync(1);

            var result = await _controller.DeleteProcess("682168c42663032aed86ff94") as OkObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsTrue(dto!.Success);
            Assert.AreEqual("Process deleted successfully", dto.Message);
        }

        [Test]
        public async Task UTCID02_DeleteProcess_MissingToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.DeleteProcess("someId");
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task UTCID03_DeleteProcess_NotExpert_ShouldReturnBadRequest()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "686c72a8a103667c96bb6000",
                RoleId = "nonExpert"
            });

            var result = await _controller.DeleteProcess("someId") as BadRequestObjectResult;
            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual("Account is not expert", dto.Message);
        }

        [Test]
        public async Task UTCID04_DeleteProcess_ProcessNotFound_ShouldReturnBadRequest()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(new UserClaimsResponseDTO
            {
                AccId = "invalid_id",
                RoleId = "68007b2a87b41211f0af1d57"
            });

            var result = await _controller.DeleteProcess("") as BadRequestObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Invalid AccIds.", result.Value);
        }

        [Test]
        public async Task UTCID05_DeleteProcess_ServiceNotLinkService_ShouldReturnBadRequest()
        {
            SetExpertUser();
            _processRepoMock.Setup(x => x.DeleteProcess("someId")).ReturnsAsync(0);

            var result = await _controller.DeleteProcess("someId") as BadRequestObjectResult;
            Assert.IsNotNull(result);
            var dto = result.Value as ProcessResponseDTO;
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual("Failed to delete process", dto.Message);
        }
    }
}
