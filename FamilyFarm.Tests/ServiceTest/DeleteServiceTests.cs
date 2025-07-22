using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace FamilyFarm.Tests.ServiceTest
{
    [TestFixture]
    public class DeleteServiceTests
    {
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IServiceRepository> _serviceRepoMock;
        private Mock<IProcessRepository> _processRepoMock;
        private Mock<IProcessStepRepository> _stepRepoMock;
        private ServiceController _controller;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthenticationService>();
            _serviceRepoMock = new Mock<IServiceRepository>();
            _processRepoMock = new Mock<IProcessRepository>();
            _stepRepoMock = new Mock<IProcessStepRepository>();

            var servicingService = new ServicingService(
                _serviceRepoMock.Object,
                null,
                null,
                null,
                _processRepoMock.Object,
                _stepRepoMock.Object,
                null
            );

            _controller = new ServiceController(servicingService, _authServiceMock.Object);
        }

        private UserClaimsResponseDTO GetExpertUser() => new UserClaimsResponseDTO
        {
            AccId = "681285af4ca4800a87b0990f",
            RoleId = "68007b2a87b41211f0af1d57"
        };

        [Test]
        public async Task UTCID01_DeleteService_Success()
        {
            var user = GetExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);
            _serviceRepoMock.Setup(x => x.DeleteService("681285af4ca4800a87b0990f")).ReturnsAsync(1);
            _processRepoMock.Setup(x => x.GetProcessByServiceId("681285af4ca4800a87b0990f")).ReturnsAsync((Process)null);

            var result = await _controller.DeleteService("681285af4ca4800a87b0990f") as OkObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ServiceResponseDTO;
            Assert.IsTrue(dto!.Success);
            Assert.AreEqual("Service deleted successfully", dto.Message);
        }

        [Test]
        public async Task UTCID02_DeleteService_MissingToken_ShouldReturnUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.DeleteService("681285af4ca4800a87b0990f") as UnauthorizedObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);
        }

        [Test]
        public async Task UTCID03_DeleteService_NotExpert_ShouldReturnBadRequest()
        {
            var user = new UserClaimsResponseDTO
            {
                AccId = "681285af4ca4800a87b0990f",
                RoleId = "someOtherRole"
            };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.DeleteService("681285af4ca4800a87b0990f") as BadRequestObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ServiceResponseDTO;
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual("Account is not expert", dto.Message);
        }

        [Test]
        public async Task UTCID04_DeleteService_InvalidServiceId_ShouldReturnBadRequest()
        {
            var user = GetExpertUser();
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(user);

            var result = await _controller.DeleteService("") as BadRequestObjectResult;

            Assert.IsNotNull(result);
            var dto = result.Value as ServiceResponseDTO;
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual("Service not found", dto.Message);
        }
    }
}
