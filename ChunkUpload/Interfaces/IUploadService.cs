using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ChunkUpload.Interfaces
{
    public interface IUploadService
    {
        Task<bool> UploadAsync(HttpRequest request, ModelStateDictionary modelState);
    }
}
