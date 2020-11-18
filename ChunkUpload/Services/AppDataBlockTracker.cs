using ChunkUpload.Abstract;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChunkUpload.Services
{
    public class AppDataBlockTracker : BlockTracker
    {
        private Dictionary<string, int> _dictionary;
        
        private readonly string _fileName;
        private readonly IWebHostEnvironment _environment;

        public AppDataBlockTracker(IWebHostEnvironment environment, string fileName)
        {
            _environment = environment;
            _fileName = System.IO.Path.Combine(_environment.ContentRootPath, "App_Data", fileName);
            
            if (System.IO.File.Exists(_fileName))
            {
                var json = System.IO.File.ReadAllText(_fileName);
                _dictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            }
            else
            {
                _dictionary = new Dictionary<string, int>();
            }
        }

        private string GetKey(string userName, string fileName) => $"{userName}.{fileName}";

        protected override async Task<int> IncrementBlockAsync(string userName, string fileName)
        {
            var key = GetKey(userName, fileName);
            if (!_dictionary.ContainsKey(key)) _dictionary.Add(key, 0);
            _dictionary[key] += 1;
            var json = JsonSerializer.Serialize(_dictionary);
            System.IO.File.WriteAllText(_fileName, json);
            return await Task.FromResult(_dictionary[key]);
        }

        protected override async Task<IEnumerable<int>> QueryBlocksAsync(string userName, string fileName)
        {
            var blockIds = Enumerable.Range(0, _dictionary[GetKey(userName, fileName)]).Select(i => i);
            return await Task.FromResult(blockIds);
        }
    }
}
