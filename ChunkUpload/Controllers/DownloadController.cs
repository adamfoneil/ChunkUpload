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
            
            var builder = new BlobSasBuilder(BlobSasPermissions.Read, DateTime.Now.AddDays(1))
            {
                BlobContainerName = containerName,
                BlobName = name,
                ContentDisposition = $"attachment; filename={name}"
            };

            return Content(blob.GenerateSasUri(builder).AbsoluteUri, "text/plain");
        }
    }
}
