using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.BusinessLogic
{
    public interface IUploadFileService
    {
        Task<FileUploadResponseDTO> UploadImage(IFormFile fileImage);
        Task<FileUploadResponseDTO> UploadOtherFile(IFormFile file);
        Task<List<FileUploadResponseDTO>> UploadListImage(List<IFormFile> files);
        Task<FileUploadResponseDTO> UploadImageFromStream(Stream stream, string originalFileName);
        Task<Stream?> DownloadImageFromUrl(string? url);
        Task<bool> DeleteFile(string? urlFile);
    }
}
