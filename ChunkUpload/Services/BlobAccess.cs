using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace ChunkUpload.Services
{
    public class BlobAccess
    {
        private readonly string _connectionString;        

        public BlobAccess(string connectionString)
        {
            _connectionString = connectionString;            
        }

        public BlockBlobClient GetBlockBlobClient(string containerName, string blobName) => new BlockBlobClient(_connectionString, containerName, blobName);

        public BlobContainerClient GetContainerClient(string containerName) => new BlobContainerClient(_connectionString, containerName);
    }
}
