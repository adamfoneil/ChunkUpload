using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using ChunkUpload.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ChunkUpload.Controllers
{
    public class DownloadController : Controller
    {
        private readonly BlobAccess _blobAccess;

        const string containerName = "private";

        public DownloadController(BlobAccess blobAccess)
        {
            _blobAccess = blobAccess;
        }

        public ContentResult GetSasUrl([FromQuery]string name)
        {
            var blob = _blobAccess.GetBlockBlobClient(containerName, name);            
            return Content(GetSasUri(blob), "text/plain");
        }
        
        public RedirectResult Blob(string name)
        {
            var blob = _blobAccess.GetBlockBlobClient(containerName, name);            
            return Redirect(GetSasUri(blob));
        }

        private static string GetSasUri(BlockBlobClient client)
        {
            var builder = new BlobSasBuilder(BlobSasPermissions.Read, DateTime.Now.AddDays(1))
            {
                BlobContainerName = containerName,
                BlobName = client.Name,
                ContentDisposition = $"attachment; filename={client.Name}"
            };

            return client.GenerateSasUri(builder).AbsoluteUri;
        }
    }
}
