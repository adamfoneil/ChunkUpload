using Azure.Storage.Blobs.Specialized;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AzureUploader.Abstract
{
    public abstract class BlockTracker 
    {
        public async Task<string> AddBlockAsync(string userName, string fileName)
        {
            int blockId = await IncrementBlockAsync(userName, fileName);
            return ToBase64(blockId);
        }

        public abstract Task CompleteFileAsync(string userName, string fileName, BlockBlobClient blobClient);
        
        protected abstract Task<int> IncrementBlockAsync(string userName, string fileName);

        public abstract Task<bool> HasUploadsInProgress(string userName);

        private string ToBase64(int id) => Convert.ToBase64String(Encoding.UTF8.GetBytes(id.ToString("d6")));
    }
}
