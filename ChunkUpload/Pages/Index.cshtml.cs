using ChunkUpload.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace ChunkUpload.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IFileStorage _storage;

        public IndexModel(IFileStorage storage)
        {
            _storage = storage;
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            foreach (var file in Request.Form.Files)
            {
                using (var stream = file.OpenReadStream())
                {
                    await _storage.AppendChunk(stream, file.FileName);
                }
            }

            return new OkResult();
        }
    }
}
