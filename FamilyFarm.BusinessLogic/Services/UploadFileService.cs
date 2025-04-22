using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Config;
using FamilyFarm.BusinessLogic.Interfaces;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.BusinessLogic.Services
{
    public class UploadFileService : FirebaseConnection, IUploadFileService
    {
        //Upload image to folder image on Firebase Storage
        public async Task<string> UploadImage(IFormFile fileImage)
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

            return imageUrl;
        }
    }
}
