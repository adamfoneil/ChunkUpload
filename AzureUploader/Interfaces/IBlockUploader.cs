using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AzureUploader.Services
{
    public interface IBlockUploader
    {
        Task CommitAsync(string userName, string fileName);        
        Task StageAsync(string userName, HttpRequest request);
    }
}