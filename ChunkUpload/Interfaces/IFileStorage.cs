using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChunkUpload.Interfaces
{
    public interface IFileStorage
    {
        Task AppendChunk(Stream content, string name);

        Task CopyTo(string name, string newFolder);

        IEnumerable<Uri> ListContents();

        bool SupportsDownload { get; }

        Task<Stream> Download(string name);
    }
}
