using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureUploader.Interfaces
{
    /// <summary>
    /// provides a way to avoid using MemoryStream
    /// </summary>
    public interface ITempFileProvider
    {
        Task ExecuteAsync(byte[] byteArray, Func<Stream, Task> executeWithStream);
    }
}
