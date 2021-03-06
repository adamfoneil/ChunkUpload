﻿using ChunkUpload.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public bool SupportsDownload => false;

        public async Task UploadAsync(string name, Stream content)
        {
            var fileName = Path.Combine(BasePath, name);
            await using var localFile = new FileStream(fileName, FileMode.Append);
            await content.CopyToAsync(localFile);
        }

        public async Task AppendChunk(Stream content, string name)
        {
            string fileName = Path.Combine(BasePath, name);

            using (var localFile = new FileStream(fileName, FileMode.Append))
            {
                await content.CopyToAsync(localFile);
            }
        }

        public Task<Stream> Download(string name) => throw new NotImplementedException();

        public IEnumerable<Uri> ListContents()
        {
            return Directory.GetFiles(BasePath, "*", SearchOption.TopDirectoryOnly).Select(fileName => new Uri($"file://{fileName}"));
        }

        public Task CopyTo(string name, string newFolder) => throw new NotImplementedException();
    }
}
