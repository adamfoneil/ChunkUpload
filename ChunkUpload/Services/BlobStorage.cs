using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using ChunkUpload.Interfaces;
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

        public BlobStorage(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            ContainerName = containerName;            
        }

        public string ContainerName { get; }

        public bool SupportsDownload => true;

        public async Task AppendChunk(Stream content, string name)
        {
            var container = new BlobContainerClient(_connectionString, ContainerName);
            await container.CreateIfNotExistsAsync();

            var appendBlob = new AppendBlobClient(_connectionString, ContainerName, name);
            await appendBlob.CreateIfNotExistsAsync();
            await appendBlob.AppendBlockAsync(content);
        }

        public async Task<byte[]> Download(string name)
        {
            var client = new BlockBlobClient(_connectionString, ContainerName, name);

            // might be dangerous for really large files, not sure
            using (var ms = new MemoryStream())
            {
                await client.DownloadToAsync(ms);
                return ms.ToArray();
            }
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
            var props = await srcBlob.GetPropertiesAsync();
            var download = await srcBlob.DownloadAsync();

            var destContainer = new BlobContainerClient(_connectionString, newFolder);
            await destContainer.CreateIfNotExistsAsync();            

            var destBlob = destContainer.GetBlockBlobClient(name);
            await destBlob.UploadAsync(download.Value.Content);

            sw.Stop();

            Debug.WriteLine($"Copied {srcBlob.Name} from {srcBlob.BlobContainerName} to {newFolder} in {sw.ElapsedMilliseconds} ms ({getTransferRate():n0} bytes/sec)");

            decimal getTransferRate() => (props.Value.ContentLength / (decimal)sw.ElapsedMilliseconds) * 1000;
        }
    }
}
