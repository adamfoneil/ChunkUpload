using ChunkUpload.Attributes;
using ChunkUpload.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChunkUpload.Controllers
{
    public class FilesController : Controller
    {
        //private readonly IUploadService _uploadService;
        private readonly BlockBlobUploader _uploader;

        public FilesController(BlockBlobUploader uploader)
        {
            //_uploadService = uploadService;
            _uploader = uploader;
        }

        [HttpPost]
        //[DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        //[RequestSizeLimit(int.MaxValue)]
        //[RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadAsync()
        {
            await _uploader.StageAsync("default", Request);
            /*var result = await _uploadService.UploadAsync(Request, ModelState);
            if (!result)
            {
                return BadRequest(ModelState);
            }*/

            return new OkResult();
        }

        [HttpPost]
        public async Task<IActionResult> FinishFileAsync([FromQuery]string fileName)
        {
            await _uploader.CommitAsync("default", fileName);
            return new OkResult();
        }    
    }
}
