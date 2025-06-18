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


        //Method delete file on Firebase based on url in database
        public async Task<bool> DeleteFile(string? urlFile)
        {
            if (string.IsNullOrEmpty(urlFile))
                return false;

            try
            {
                var filePath = GetFilePathFromUrl(urlFile);

                var storage = new FirebaseStorage(
                    "prn221-69738.appspot.com",
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(""),
                        ThrowOnCancel = true
                    });

                await storage
                    .Child(filePath)
                    .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
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

        public async Task<List<FileUploadResponseDTO>> UploadListImage(List<IFormFile> files)
        {
            var uploadResults = new List<FileUploadResponseDTO>();

            var storage = new FirebaseStorage(
                "prn221-69738.appspot.com",
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(""),
                    ThrowOnCancel = true
                });

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue; // Bỏ qua file null hoặc file rỗng

                var stream = file.OpenReadStream();
                var fileName = $"image/{DateTime.UtcNow.Ticks}_{file.FileName}";

                var imageUrl = await storage
                    .Child(fileName)
                    .PutAsync(stream);

                uploadResults.Add(new FileUploadResponseDTO
                {
                    Message = "Upload image successfully.",
                    UrlFile = imageUrl,
                    TypeFile = "image",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return uploadResults;
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

        //Method get file name in Url file 
        private string GetFilePathFromUrl(string urlFile)
        {
            var uri = new Uri(urlFile);
            var path = Uri.UnescapeDataString(uri.AbsolutePath); // Giải mã %2F -> /

            var index = path.IndexOf("/o/");
            if (index == -1)
                throw new Exception("Invalid Firebase Storage URL.");

            var filePath = path.Substring(index + 3); // Lấy phần sau "/o/"

            return filePath;
        }

        //Download image from url
        public async Task<Stream?> DownloadImageFromUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsStreamAsync();
            }
            catch
            {
                return null;
            }
        }

        //Updoad image downloaded from url to firebase
        public async Task<FileUploadResponseDTO> UploadImageFromStream(Stream stream, string originalFileName)
        {
            var fileName = $"image/{DateTime.UtcNow.Ticks}_{originalFileName}";

            var storage = new FirebaseStorage(
                "prn221-69738.appspot.com",
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(""),
                    ThrowOnCancel = true
                });

            var imageUrl = await storage
                .Child(fileName)
                .PutAsync(stream);

            return new FileUploadResponseDTO
            {
                Message = "Upload image from stream successfully.",
                UrlFile = imageUrl,
                TypeFile = "image",
                CreatedAt = DateTime.UtcNow
            };
        }

    }
}
