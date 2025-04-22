using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    interface IUploadFileService
    {
        Task<string> UploadImage(IFormFile fileImage);
    }
}
