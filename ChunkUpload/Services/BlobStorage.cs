using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using ChunkUpload.Extensions;
using ChunkUpload.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChunkUpload.Services
{
    public class BlobStorage : IFileStorage
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public BlobStorage(string connectionString, string containerName, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            ContainerName = containerName;
            _logger = loggerFactory.CreateLogger(nameof(BlobStorage));
        }

        public string ContainerName { get; }

        public bool SupportsDownload => true;

        public async Task UploadAsync(string name, Stream content)
        {
            var container = new BlobContainerClient(_connectionString, ContainerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlobClient(name);
            await blob.UploadAsync(content);
        }

        public async Task AppendChunk(Stream content, string name)
        {
            var container = new BlobContainerClient(_connectionString, ContainerName);
            await container.CreateIfNotExistsAsync();

            var appendBlob = new AppendBlobClient(_connectionString, ContainerName, name);
            await appendBlob.CreateIfNotExistsAsync();
            await appendBlob.AppendBlockAsync(content);
        }

        public async Task<Stream> Download(string name)
        {
            var client = new BlockBlobClient(_connectionString, ContainerName, name);
            var result = await client.DownloadAsync();
            return result.Value.Content;
        }

        public IEnumerable<Uri> ListContents()
        {
            var container = new BlobContainerClient(_connectionString, ContainerName);
            var result = container.GetBlobs();
            return result.Select(item => new Uri(container.Uri + "/" + item.Name));
        }

        /// <summary>
        /// effectively "converts" an Append blob type to a Block type through a download and upload
        /// </summary>
        public async Task CopyTo(string name, string newFolder)
        {
            var sw = Stopwatch.StartNew();

            var srcContainer = new BlobContainerClient(_connectionString, ContainerName);
            var srcBlob = srcContainer.GetAppendBlobClient(name);
            var download = await srcBlob.DownloadAsync();

            var destContainer = new BlobContainerClient(_connectionString, newFolder);
            await destContainer.CreateIfNotExistsAsync();

            var destBlob = destContainer.GetBlockBlobClient(name);

            try
            {
                await destBlob.UploadAsync(download.Value.Content);
            }
            finally
            {
                download.Value.Dispose();
            }

            sw.Stop();

            _logger?.LogInformation($"Copied {srcBlob.Name} ({Extensions.Readable.FileSize(download.Value.ContentLength)}) from {srcBlob.BlobContainerName} to {newFolder} in {(sw.ElapsedMilliseconds / 1000m):n1} sec ({getTransferRate():n0} bytes/sec)");

            decimal getTransferRate() => (download.Value.ContentLength / (decimal)sw.ElapsedMilliseconds) * 1000;
        }
    }
}
