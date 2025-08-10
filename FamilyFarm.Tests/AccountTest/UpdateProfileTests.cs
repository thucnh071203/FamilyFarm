using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.BusinessLogic.Services;
using FamilyFarm.BusinessLogic;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.BusinessLogic.PasswordHashing;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.BusinessLogic.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FamilyFarm.Tests.AccountTests
{
    //public class FileUploadResult
    //{
    //    public string? UrlFile { get; set; }
    //}

    [TestFixture]
    public class UpdateProfileTests
    {
        private Mock<IAccountRepository> _accountRepoMock;
        private Mock<IUploadFileService> _uploadMock;
        private IAccountService _service;
        private IMapper _mapper;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;
        private Account CreateMockAccount(string id) => new Account
        {
            AccId = id,
            RoleId = "role-id",
            Username = "testuser",
            PasswordHash = "hashed",
            FullName = "Test User",
            Email = "test@example.com",
            PhoneNumber = "0123456789",
            City = "Hanoi",
            Country = "Vietnam",
            Status = 0,
            CreatedAt = DateTime.UtcNow
        };

        [SetUp]
        public void Setup()
        {
            _accountRepoMock = new Mock<IAccountRepository>();
            _uploadMock = new Mock<IUploadFileService>();

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<Account, MyProfileDTO>();
            });
            _mapper = config.CreateMapper();

            _service = new AccountService(_accountRepoMock.Object, new PasswordHasher(), _uploadMock.Object, _mapper, _hubContext);
        }

        [Test]
        public async Task TC01_ValidExpertData_ReturnsSuccess()
        {
            var id = "acc123";
            var dto = new UpdateProfileRequestDTO { FullName = "Người dùng 1", City = "HN", Country = "VN", Email = "abc@gmail.com" };
            _accountRepoMock.Setup(x => x.GetAccountById(id)).ReturnsAsync(CreateMockAccount(id));
            _accountRepoMock.Setup(x => x.UpdateAsync(id, It.IsAny<Account>())).ReturnsAsync(CreateMockAccount(id));

            var result = await _service.UpdateProfileAsync(id, dto);

            Assert.Multiple(() => {
                Assert.IsTrue(result.IsSuccess);
                Assert.IsNull(result.MessageError);
            });
        }

        [Test]
        public async Task TC02_NullToken_ReturnsUnauthorized()
        {
            var result = await _service.UpdateProfileAsync(null, new UpdateProfileRequestDTO { FullName = "A", City = "A", Country = "B" });
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public async Task TC03_EmptyFullName_ReturnsBadRequest()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "", City = "HN", Country = "VN" };
            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public async Task TC04_EmailAlreadyExists_ReturnsError()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Email = "duplicate@example.com" };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.GetAccountByEmail("duplicate@example.com")).ReturnsAsync(CreateMockAccount("acc999"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Email already in use.", result.MessageError);
        }

        [Test]
        public async Task TC05_PhoneAlreadyExists_ReturnsError()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", PhoneNumber = "0912345678" };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.GetAccountByPhone("0912345678")).ReturnsAsync(CreateMockAccount("acc999"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Phone number already in use.", result.MessageError);
        }

        [Test]
        public async Task TC06_ValidFilesUploadedSuccessfully()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Avatar = Mock.Of<IFormFile>() };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            //_uploadMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ReturnsAsync(new FileUploadResult { UrlFile = "http://file.com/avatar.png" });
            _uploadMock
            .Setup(x => x.UploadImage(It.IsAny<IFormFile>()))
            .ReturnsAsync(new FileUploadResponseDTO
            {
                UrlFile = "http://file.com/avatar.png",
                Message = "Success",
                TypeFile = "image",
                CreatedAt = DateTime.UtcNow
            });
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC07_UploadedFileInvalid_KeepsOldAvatar()
        {
            var acc = CreateMockAccount("acc123");
            acc.Avatar = "old-avatar";
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Avatar = Mock.Of<IFormFile>() };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(acc);
            //_uploadMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ReturnsAsync((FileUploadResult)null);
            _uploadMock
            .Setup(x => x.UploadImage(It.IsAny<IFormFile>()))
            .ReturnsAsync((FileUploadResponseDTO?)null);
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(acc);

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC08_AccountNotFound_ReturnsError()
        {
            _accountRepoMock.Setup(x => x.GetAccountById("not-exist")).ReturnsAsync((Account)null);
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN" };

            var result = await _service.UpdateProfileAsync("not-exist", dto);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Account not found", result.MessageError);
        }

        [Test]
        public async Task TC09_BirthdayNull_StillSuccess()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Birthday = null };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC10_EmailInvalidFormat_StillProcessed()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Email = "abc" };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC11_PhoneInvalidFormat_StillProcessed()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", PhoneNumber = "abc" };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC12_EmailPhoneEmpty_StillSuccess()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Email = "", PhoneNumber = "" };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC13_UploadFileReturnsEmpty_KeepsOldValue()
        {
            var acc = CreateMockAccount("acc123");
            acc.Background = "old-bg";
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Background = Mock.Of<IFormFile>() };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(acc);
            _uploadMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ReturnsAsync(new FileUploadResponseDTO { UrlFile = null });
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(acc);

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC14_RequestIsNull_ReturnsError()
        {
            var result = await _service.UpdateProfileAsync("acc123", null);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Request is null", result.MessageError);
        }

        [Test]
        public async Task TC15_FullDataWithWorkAtStudyAt_ReturnsSuccess()
        {
            var dto = new UpdateProfileRequestDTO
            {
                FullName = "Người dùng 1",
                City = "HN",
                Country = "VN",
                WorkAt = "FPT",
                StudyAt = "Bach Khoa"
            };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC16_EmailIsNull_ShouldSucceedIfOtherValid()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Email = null };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC17_PhoneIsNull_ShouldSucceedIfOtherValid()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", PhoneNumber = null };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC18_BackgroundFileIsEmpty_ShouldKeepOld()
        {
            var acc = CreateMockAccount("acc123");
            acc.Background = "old-bg-url";
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Background = Mock.Of<IFormFile>() };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(acc);
            _uploadMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ReturnsAsync((FileUploadResponseDTO?)null);
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(acc);

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC19_CertificateFileIsEmpty_ShouldKeepOld()
        {
            var acc = CreateMockAccount("acc123");
            acc.Certificate = "old-cert";
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Certificate = Mock.Of<IFormFile>() };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(acc);
            _uploadMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ReturnsAsync((FileUploadResponseDTO?)null);
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(acc);

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC20_InvalidGender_ShouldStillUpdate()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Gender = "Alien" };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC21_AddressIsNull_ShouldStillUpdate()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Address = null };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(CreateMockAccount("acc123"));

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC22_UploadImageThrowsException_ShouldCatchAndReturnOldValue()
        {
            var acc = CreateMockAccount("acc123");
            acc.Avatar = "old-avatar";
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN", Avatar = Mock.Of<IFormFile>() };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(acc);
            _uploadMock.Setup(x => x.UploadImage(It.IsAny<IFormFile>())).ThrowsAsync(new Exception("Firebase error"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync(acc);

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async Task TC23_RepositoryUpdateFails_ReturnsFail()
        {
            var dto = new UpdateProfileRequestDTO { FullName = "User", City = "HN", Country = "VN" };
            _accountRepoMock.Setup(x => x.GetAccountById("acc123")).ReturnsAsync(CreateMockAccount("acc123"));
            _accountRepoMock.Setup(x => x.UpdateAsync("acc123", It.IsAny<Account>())).ReturnsAsync((Account?)null);

            var result = await _service.UpdateProfileAsync("acc123", dto);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Update fail!", result.MessageError);
        }
    }
}
