using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ChunkUpload.Interfaces
{
    public interface IBlockBlobUploader
    {
        Task StageAsync(string userName, HttpRequest request);
        Task CommitAsync(string userName, string fileName);
    }
}
