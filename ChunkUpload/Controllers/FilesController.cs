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
        public async Task<IActionResult> Commit([FromForm]string fileName)
        {            
            await _uploader.CommitAsync("default", fileName);            
            return new OkResult();
        }    
    }
}
