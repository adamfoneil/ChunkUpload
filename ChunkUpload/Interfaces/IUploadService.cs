using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace ChunkUpload.Interfaces
{
    public interface IUploadService
    {
        Task<bool> UploadAsync(HttpRequest request, ModelStateDictionary modelState);
    }
}
