using AzureUploader.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChunkUpload.Testing
{
    [TestClass]
    public class UploaderTests
    {
        [TestMethod]
        public void BlobNameValidation()
        {
            Assert.IsTrue(BlockBlobUploader.BlobName("hello", "this.xlsx").Equals("hello/this.xlsx"));
            Assert.IsTrue(BlockBlobUploader.BlobName(null, "this.xlsx").Equals("this.xlsx"));
        }
    }
}
