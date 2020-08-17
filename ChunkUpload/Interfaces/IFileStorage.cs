using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChunkUpload.Interfaces
{
    public interface IFileStorage
    {
        Task AppendChunk(Stream content, string name);
        
        IEnumerable<Uri> ListContents();

        bool SupportsDownload { get; }

        Task<byte[]> Download(string name);
    }
}
