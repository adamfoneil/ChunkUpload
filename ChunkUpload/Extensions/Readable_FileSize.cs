using System.Runtime.InteropServices;
using System.Text;

namespace ChunkUpload.Extensions
{
    public static partial class Readable
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

        /// <summary>
        /// help from https://stackoverflow.com/a/281716/2023653
        /// </summary>
        public static string FileSize(long fileSize)
        {
            var sb = new StringBuilder(11);
            StrFormatByteSize(fileSize, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
