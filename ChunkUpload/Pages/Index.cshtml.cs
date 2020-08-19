using ChunkUpload.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChunkUpload.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<Uri> ExistingFiles { get; set; }

        public IndexModel(IFileStorage storage)
        {
            Storage = storage;
        }

        public IFileStorage Storage { get; }

        public async void OnGet()
        {
            ExistingFiles = Storage.ListContents();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            foreach (var file in Request.Form.Files)
            {
                using (var stream = file.OpenReadStream())
                {
                    await Storage.AppendChunk(stream, file.FileName);
                }
            }

            return new OkResult();
        }

        public async Task<FileResult> OnPostDownloadAsync(string name)
        {
            // the incoming name has the container name, which we need to remove here
            string localFile = Path.GetFileName(name);

            return File(await Storage.Download(localFile), "application/octet-stream", localFile);
        }

        public async Task<IActionResult> OnPostMoveAsync(string name, string container)
        {
            // the incoming name has the container name, which we need to remove here
            string localFile = Path.GetFileName(name);

            await Storage.CopyTo(localFile, container);

            return new OkResult();
        }
    }
}
