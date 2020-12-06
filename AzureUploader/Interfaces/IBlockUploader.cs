using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AzureUploader.Services
{
    public interface IBlockUploader
    {
        Task StageAsync(string userName, HttpRequest request, string prefix = null);
        Task CommitAsync(string userName, string fileName, string prefix = null);                
    }
}