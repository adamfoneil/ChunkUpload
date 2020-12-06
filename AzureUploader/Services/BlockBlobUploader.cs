﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using AzureUploader.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Linq;
using System.Threading.Tasks;

namespace AzureUploader.Services
{
    /// <summary>
    /// Perform chunked uploads to Azure blob storage
    /// </summary>
    public class BlockBlobUploader : IBlockUploader
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly BlockTracker _blockTracker;

        public BlockBlobUploader(string connectionString, string containerName, BlockTracker blockTracker)
        {
            _connectionString = connectionString;
            _containerName = containerName;
            _blockTracker = blockTracker;
        }        

        public async Task StageAsync(string userName, HttpRequest request, string prefix = null)
        {
            var containerClient = new BlobContainerClient(_connectionString, _containerName);
            await containerClient.CreateIfNotExistsAsync();            

            foreach (var file in request.Form.Files)
            {
                string blobName = BlobName(prefix, file.FileName);
                var client = new BlockBlobClient(_connectionString, _containerName, blobName);
                using (var stream = file.OpenReadStream())
                {
                    var blockId = await _blockTracker.AddBlockAsync(userName, blobName);
                    await client.StageBlockAsync(blockId, stream);
                }
            }
        }

        public async Task CommitAsync(string userName, string fileName, string prefix = null)
        {
            string blobName = BlobName(prefix, fileName);
            var blobClient = new BlockBlobClient(_connectionString, _containerName, blobName);
            var blockList = (await blobClient.GetBlockListAsync()).Value;
            var blockIds = blockList.UncommittedBlocks.Select(block => block.Name);

            await blobClient.CommitBlockListAsync(blockIds, new BlobHttpHeaders()
            {
                ContentType = GetContentType()
            });

            await _blockTracker.CompleteFileAsync(userName, blobName);

            string GetContentType()
            {
                var provider = new FileExtensionContentTypeProvider();
                return (provider.TryGetContentType(fileName, out string contentType)) ? contentType : "application/octet-stream";
            }
        }

        /// <summary>
        /// added this so I could delete and retry uploads that were canceled or not done right when I was figuring out how this worked
        /// </summary>        
        public async Task ResolveUncommittedAsync()
        {
            var containerClient = new BlobContainerClient(_connectionString, _containerName);
            var results = containerClient.GetBlobsAsync(traits: BlobTraits.None, states: BlobStates.Uncommitted);

            await foreach (var page in results.AsPages())
            {
                foreach (var item in page.Values)
                {
                    var blobClient = new BlockBlobClient(_connectionString, _containerName, item.Name);

                    try
                    {
                        var blockList = await blobClient.GetBlockListAsync();
                        var blockIds = blockList.Value.UncommittedBlocks.Select(block => block.Name);
                        await blobClient.CommitBlockListAsync(blockIds);
                    }
                    catch
                    {
                        // ignore, traits filter doesn't seem to work, so I just try/catch around the invalid blobs
                    }
                }
            }
        }

        public static string BlobName(string prefix, string fileName) => string.Join("/", new string[]
        {
            prefix, fileName
        }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
