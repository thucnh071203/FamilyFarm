using FamilyFarm.BusinessLogic;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFarm.API.Controllers
{
    [Route("api/firebase")]
    [ApiController]
    [Authorize]
    public class UploadCloudController : ControllerBase
    {
        private readonly IUploadFileService _uploadService;

        public UploadCloudController(IUploadFileService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost("upload-image")]
        public async Task<ActionResult<FileUploadResponseDTO>> UploadImage([FromForm] FileUploadRequestDTO request)
        {
            if (request.imageFile == null || request.imageFile.Length == 0)
                return BadRequest(new FileUploadResponseDTO
                {
                    Message = "File is not valid."
                });

            var result = await _uploadService.UploadImage(request.imageFile);
            return Ok(result);
        }

        [HttpPost("upload-files")]
        public async Task<ActionResult<FileUploadResponseDTO>> UploadOtherFiles([FromForm] FileUploadRequestDTO request)
        {
            if (request.otherFile == null || request.otherFile.Length == 0)
                return BadRequest(new FileUploadResponseDTO
                {
                    Message = "File is not valid."
                });

            var result = await _uploadService.UploadOtherFile(request.otherFile);
            return Ok(result);
        }
    }
}
