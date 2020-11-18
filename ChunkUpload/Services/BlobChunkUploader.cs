using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ChunkUpload.Abstract;
using ChunkUpload.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ChunkUpload.Services
{
    public class BlobChunkUploader : IChunkUploader
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly BlockTracker _blockTracker;

        public BlobChunkUploader(string connectionString, string containerName, BlockTracker blockTracker)
        {
            _connectionString = connectionString;
            _containerName = containerName;
            _blockTracker = blockTracker;
        }        

        public async Task UploadChunkAsync(string userName, HttpRequest request)
        {
            var containerClient = new BlobContainerClient(_connectionString, _containerName);
            await containerClient.CreateIfNotExistsAsync();

            foreach (var file in request.Form.Files)
            {
                var client = new BlockBlobClient(_connectionString, _containerName, file.FileName);
                using (var stream = file.OpenReadStream())
                {
                    var blockId = await _blockTracker.AddBlockAsync(userName, file.FileName);
                    await client.StageBlockAsync(blockId, stream);
                }
            }
        }

        public async Task CompleteFileAsync(string userName, string fileName, string contentType)
        {
            var client = new BlockBlobClient(_connectionString, _containerName, fileName);
            var blocksIds = await _blockTracker.GetAllBlocksAsync(userName, fileName);
            await client.CommitBlockListAsync(blocksIds, new BlobHttpHeaders()
            {
                ContentType = contentType
            });
        }
    }
}
