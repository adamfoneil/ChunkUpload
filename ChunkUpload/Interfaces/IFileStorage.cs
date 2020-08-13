using System.IO;
using System.Threading.Tasks;

namespace ChunkUpload.Interfaces
{
    public interface IFileStorage
    {
        Task AppendChunk(Stream content, string name);
    }
}
