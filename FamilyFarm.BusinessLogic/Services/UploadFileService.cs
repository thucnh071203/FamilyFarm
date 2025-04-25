using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Config;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FamilyFarm.BusinessLogic.Services
{
    public class UploadFileService : FirebaseConnection, IUploadFileService
    {
        public UploadFileService(IConfiguration configuration) : base(configuration)
        {
        }

        //Upload image to folder image on Firebase Storage
        public async Task<FileUploadResponseDTO> UploadImage(IFormFile fileImage)
        {
            var stream = fileImage.OpenReadStream();
            var fileName = $"image/{DateTime.UtcNow.Ticks}_{fileImage.FileName}";


            var storage = new FirebaseStorage(
            "prn221-69738.appspot.com",
            new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(""),
                ThrowOnCancel = true
            });

            // Upload file lên Firebase Storage và lấy URL
            var imageUrl = await storage
                .Child(fileName)
                .PutAsync(stream);

            return new FileUploadResponseDTO
            {
                Message = "Upload image is successfully.",
                UrlFile = imageUrl,
                TypeFile = "image",
                CreatedAt = DateTime.UtcNow   
            };
        }

        public async Task<FileUploadResponseDTO> UploadOtherFile(IFormFile file)
        {
            var stream = file.OpenReadStream();
            var fileName = $"other/{DateTime.UtcNow.Ticks}_{file.FileName}";


            var storage = new FirebaseStorage(
            "prn221-69738.appspot.com",
            new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(""),
                ThrowOnCancel = true
            });

            // Upload file lên Firebase Storage và lấy URL
            var fileUrl = await storage
                .Child(fileName)
                .PutAsync(stream);

            return new FileUploadResponseDTO
            {
                Message = "Upload file is successfully.",
                UrlFile = fileUrl,
                TypeFile = file.ContentType,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
