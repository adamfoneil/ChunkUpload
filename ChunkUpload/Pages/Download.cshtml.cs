using Azure.Storage.Blobs.Models;
using ChunkUpload.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChunkUpload.Pages
{
    public class DownloadModel : PageModel
    {
        private readonly BlobAccess _blobAccess;
        private static HttpClient _client = new HttpClient();

        const string containerName = "private";

        public IEnumerable<BlobItem> BlobItems { get; private set; }

        public DownloadModel(BlobAccess blobAccess)
        {
            _blobAccess = blobAccess;
        }

        public async Task OnGetAsync()
        {
            var container = _blobAccess.GetContainerClient(containerName);
            var blobs = container.GetBlobsAsync().AsPages();

            List<BlobItem> items = new List<BlobItem>();
            await foreach (var page in blobs) items.AddRange(page.Values);
            BlobItems = items;
        }

        public async Task<FileResult> OnPostOpenReadAsync(string name)
        {
            // works, but doesn't seem to return memory when done
            var blob = _blobAccess.GetBlockBlobClient(containerName, name);
            var props = await blob.GetPropertiesAsync();
            return File(await blob.OpenReadAsync(), props.Value.ContentType, blob.Name);
        }

        public async Task<FileResult> OnPostDownload(string name)
        {
            // doesn't work because the "middle-man" download takes too long
            var blob = _blobAccess.GetBlockBlobClient(containerName, name);
            var uri = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTime.Now.AddDays(1)).AbsoluteUri;
            var props = await blob.GetPropertiesAsync();
            
            var result = await _client.GetAsync(uri);
            result.EnsureSuccessStatusCode();            
            return File(await result.Content.ReadAsByteArrayAsync(), props.Value.ContentType);            
        }        
    }
}
