using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AzureUploader.Services
{
    public interface IBlockUploader
    {
        Task StageAsync(string userName, HttpRequest request);
        Task CommitAsync(string userName, string fileName);                
    }
}