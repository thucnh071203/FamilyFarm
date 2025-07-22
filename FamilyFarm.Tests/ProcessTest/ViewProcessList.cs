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
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Mvc;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;

namespace FamilyFarm.Tests.ProcessTest
{
    [TestFixture]
    public class ViewProcessList
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
        public async Task GetAllProcesses_WithValidLoginAndProcesses_ReturnsOk()
        {
            // Arrange
            var account = new UserClaimsResponseDTO() { AccId = "60f7c2d7e3c6f93c2c28a1b9" }; // valid ObjectId
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(account);

            var expectedResponse = new ProcessResponseDTO
            {
                Success = true,
                Count = 1,
                Data = new List<ProcessMapper> { new ProcessMapper { process = new Process { ProcessId = "processid123" } } }
            };

            _processServiceMock.Setup(x => x.GetAllProcess()).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllProcesses();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(expectedResponse, okResult.Value);
        }

        [Test]
        public async Task GetAllProcesses_WithValidLoginButNoProcesses_ReturnsOkWithEmptyList()
        {
            var account = new UserClaimsResponseDTO() { AccId = "60f7c2d7e3c6f93c2c28a1b9" };
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns(account);

            var expectedResponse = new ProcessResponseDTO
            {
                Success = true,
                Count = 0,
                Data = new List<ProcessMapper>()
            };

            _processServiceMock.Setup(x => x.GetAllProcess()).ReturnsAsync(expectedResponse);

            var result = await _controller.GetAllProcesses();

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(expectedResponse, okResult.Value);
        }

        [Test]
        public async Task GetAllProcesses_WithNoLogin_ReturnsUnauthorized()
        {
            _authServiceMock.Setup(x => x.GetDataFromToken()).Returns((UserClaimsResponseDTO)null);

            var result = await _controller.GetAllProcesses();

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual("Invalid token or user not found.", unauthorizedResult.Value);
        }
    }
}
