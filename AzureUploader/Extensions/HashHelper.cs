using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChunkUpload.Helpers
{
    public static class HashHelper
    {
        public static string Md5(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input.ToString());
            return Md5(bytes);
        }

        public static string Md5(byte[] bytes)
        {
            using (var md5 = MD5.Create())
            {
                using (var ms = new MemoryStream(bytes))
                {
                    return Md5(ms);
                }
            }
        }

        public static string Md5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return Convert.ToBase64String(md5.ComputeHash(stream));
            }
        }
    }
}
