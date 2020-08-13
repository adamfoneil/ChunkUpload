using ChunkUpload.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChunkUpload.Services
{
    public class LocalStorage : IFileStorage
    {
        public LocalStorage(string folderName)
        {
            BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
        }

        public string BasePath { get; }

        public async Task AppendChunk(Stream content, string name)
        {
            string fileName = Path.Combine(BasePath, name);

            using (var localFile = new FileStream(fileName, FileMode.Append))
            {
                await content.CopyToAsync(localFile);
            }            
        }
    }
}
