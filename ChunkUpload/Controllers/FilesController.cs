using AzureUploader.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChunkUpload.Controllers
{
    public class FilesController : Controller
    {
        private readonly BlockBlobUploader _uploader;

        public FilesController(BlockBlobUploader uploader)
        {    
            _uploader = uploader;
        }

        [HttpPost]        
        public async Task<IActionResult> Stage()
        {
            await _uploader.StageAsync("default", Request);
            return new OkResult();
        }

        [HttpPost]
        public async Task<RedirectResult> Commit([FromForm]string fileName, [FromForm]string returnUrl)
        {            
            await _uploader.CommitAsync("default", fileName);
            TempData.Add("uploadSuccess", fileName);
            return Redirect(returnUrl);
        }    
    }
}
