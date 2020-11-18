using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json;

namespace ChunkUpload.Testing
{
    [TestClass]
    public class SerailizationTests
    {
        [TestMethod]
        public void DictionarySerialize()
        {
            var dictionary = new Dictionary<string, int>
            {
                ["this"] = 1,
                ["that"] = 2,
                ["other"] = 3
            };

            var json = JsonSerializer.Serialize(dictionary);

            var newDictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

            Assert.IsTrue(newDictionary.Equals(newDictionary));
        }
    }
}
