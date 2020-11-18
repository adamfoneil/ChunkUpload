using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ChunkUpload.Interfaces
{
    public interface IChunkUploader
    {
        Task UploadChunkAsync(string userName, HttpRequest request);
        Task CompleteFileAsync(string userName, string fileName, string contentType);
    }
}
