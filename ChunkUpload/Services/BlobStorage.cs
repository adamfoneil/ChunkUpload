using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using ChunkUpload.Interfaces;
using System;
using System.IO;
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

        public async Task AppendChunk(Stream content, string name)
        {
            var container = new BlobContainerClient(_connectionString, ContainerName);
            await container.CreateIfNotExistsAsync();

            var appendBlob = new AppendBlobClient(_connectionString, ContainerName, name);
            await appendBlob.CreateIfNotExistsAsync();
            await appendBlob.AppendBlockAsync(content);

        }
    }
}
