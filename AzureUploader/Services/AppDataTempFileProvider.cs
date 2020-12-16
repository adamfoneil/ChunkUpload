using AzureUploader.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureUploader.Services
{
    public class AppDataTempFileProvider : ITempFileProvider
    {
        private readonly string _basePath;

        private static int _fileCount = 0;

        public AppDataTempFileProvider(string basePath, string userName)
        {
            _basePath = Path.Combine(basePath, userName);
        }

        public AppDataTempFileProvider(IWebHostEnvironment environment, string userName) : this(Path.Combine(environment.ContentRootPath, "App_Data"), userName)
        {
        }
        
        public async Task ExecuteAsync(byte[] byteArray, Func<Stream, Task> executeWithStream)
        {
            if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);

            // to prevent file name collisions (not sure why this would happen)
            // we keep incrementing file index as needed
            string fileName;
            do
            {
                _fileCount++;
                fileName = Path.Combine(_basePath, $"file{_fileCount}.tmp");
            } while (File.Exists(fileName));

            try
            {
                // first write our byte array to a file, and ensure file is fully written
                using (var file = File.Create(fileName))
                {
                    await file.WriteAsync(byteArray, 0, byteArray.Length);                    
                }

                // now execute a task that requires stream access to the original byte array (such as a large blob upload)
                using (var file = File.OpenRead(fileName))
                {
                    await executeWithStream(file);
                }
            }
            finally
            {
                // even if an exception occurs, our temp file is deleted
                File.Delete(fileName);
            }
        }
    }
}
