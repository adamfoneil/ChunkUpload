using ChunkUpload.Attributes;
using ChunkUpload.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChunkUpload.Controllers
{
    public class FilesController : Controller
    {
        private readonly IUploadService _uploadService;

        public FilesController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadAsync()
        {
            var result = await _uploadService.UploadAsync(Request, ModelState);
            if (!result)
            {
                return BadRequest(ModelState);
            }

            return new OkResult();
        }
    }
}
