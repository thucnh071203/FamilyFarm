using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.API.Controllers;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic;
using Moq;
using NUnit.Framework;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Tests.ProcessTest
{
    [TestFixture]
    public class ViewProcessDetailTest
    {
        private Mock<IProcessService> _processServiceMock;
        private Mock<IAuthenticationService> _authServiceMock;
        private Mock<IUploadFileService> _uploadFileServiceMock;
        private ProcessController _controller;

        [SetUp]
        public void Setup()
        {
            _processServiceMock = new Mock<IProcessService>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _uploadFileServiceMock = new Mock<IUploadFileService>();

            _controller = new ProcessController(_processServiceMock.Object, _authServiceMock.Object, _uploadFileServiceMock.Object);
        }


        [Test]
        public async Task GetProcessByProcessId_WithValidLoginAndExistingProcess_ReturnsOk()
        {
            var processId = "6865ffa6451aa0996bb18c14";
            var expectedResponse = new ProcessOriginResponseDTO { Success = true };
            _authServiceMock.Setup(x => x.GetDataFromToken())
                .Returns(new UserClaimsResponseDTO
                {
                    AccId = "test-user-123",
                    RoleId = "some-role-id" // Nếu bạn có logic check Role thì set đúng
                });

            _processServiceMock.Setup(x => x.GetProcessByProcessId(processId)).ReturnsAsync(expectedResponse);

            var result = await _controller.GetProcessByProcessId(processId);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(expectedResponse, okResult.Value);
        }

        [Test]
        public async Task GetProcessByProcessId_WithNoLogin_ReturnsUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.GetProcessByProcessId("686d087a57140dd1344df0f3");

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task GetProcessByProcessId_WithValidLoginButEmptyProcessId_ReturnsNotFound()
        {
            // Arrange
            var account = new UserClaimsResponseDTO() { AccId = "60f7c2d7e3c6f93c2c28a1b9" }; // valid ObjectId
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(account);

            var emptyProcessId = "";
            var expectedResponse = new ProcessOriginResponseDTO { Success = false };
            _processServiceMock.Setup(x => x.GetProcessByProcessId(emptyProcessId)).ReturnsAsync(expectedResponse);

            var result = await _controller.GetProcessByProcessId(emptyProcessId);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual(expectedResponse, notFoundResult.Value);
        }

        [Test]
        public async Task GetProcessByProcessId_WithValidLoginButNonExistingProcess_ReturnsNotFound()
        {
            // Arrange
            var account = new UserClaimsResponseDTO() { AccId = "60f7c2d7e3c6f93c2c28a1b9" }; // valid ObjectId
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(account);

            var processId = "12712947892384398";
            var expectedResponse = new ProcessOriginResponseDTO { Success = false };
            _processServiceMock.Setup(x => x.GetProcessByProcessId(processId)).ReturnsAsync(expectedResponse);

            var result = await _controller.GetProcessByProcessId(processId);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual(expectedResponse, notFoundResult.Value);
        }

    }
}
