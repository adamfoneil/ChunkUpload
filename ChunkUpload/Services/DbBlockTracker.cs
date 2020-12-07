using Azure.Storage.Blobs.Specialized;
using AzureUploader.Abstract;
using Dapper;
using Dapper.CX.Extensions;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace AzureUploader.Services
{
    public class DbBlockTracker : BlockTracker
    {
        private const string schema = "log";
        private const string tableName = "BlockTracker";

        private readonly string _connectionString;

        private bool _initialized = false;

        public DbBlockTracker(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override async Task CompleteFileAsync(string userName, string fileName, BlockBlobClient client)
        {
            await InitializeAsync();

            using (var cn = GetConnection())
            {
                await cn.ExecuteAsync("DELETE [log].[BlockTracker] WHERE [UserName]=@userName AND [Filename]=@fileName", new { userName, fileName });
            }
        }

        public override async Task<bool> HasUploadsInProgress(string userName)
        {
            await InitializeAsync();

            using (var cn = GetConnection())
            {
                return await cn.RowExistsAsync("[log].[BlockTracker] WHERE [UserName]=@userName", new { userName });
            }
        }

        protected override async Task<int> IncrementBlockAsync(string userName, string fileName)
        {
            await InitializeAsync();

            var param = new { userName, fileName };

            using (var cn = GetConnection())
            {
                var affected = await cn.ExecuteAsync(
                    $"UPDATE [{schema}].[{tableName}] SET [Value]=[Value]+1 WHERE [UserName]=@userName AND [Filename]=@fileName",
                    param);

                if (affected == 0)
                {
                    await cn.ExecuteAsync(
                        @$"INSERT INTO [{schema}].[{tableName}] ([UserName], [Filename], [Value], [Timestamp]) 
                        VALUES (@userName, @fileName, 1, getutcdate())", param);
                }

                return await cn.QuerySingleAsync<int>($"SELECT [Value] FROM [{schema}].[{tableName}] WHERE [UserName]=@userName AND [Filename]=@fileName", param);
            }
        }

        private SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            using (var cn = GetConnection())
            {
                if (!await cn.SchemaExistsAsync(schema))
                {
                    await cn.ExecuteAsync($"CREATE SCHEMA [{schema}]");
                }

                if (!await cn.TableExistsAsync(schema, tableName))
                {
                    await cn.ExecuteAsync($@"CREATE TABLE [{schema}].[{tableName}] (
                        [Id] bigint identity(1,1) NOT NULL,
                        [Timestamp] datetime NOT NULL,
                        [UserName] nvarchar(50) NOT NULL,
                        [Filename] nvarchar(255) NOT NULL,
                        [Value] int NOT NULL,
                        CONSTRAINT [PK_log_BlockTracker] PRIMARY KEY ([UserName], [Filename]),
                        CONSTRAINT [U_log_BlockTracker] UNIQUE ([Id])
                    )");
                }
            }

            _initialized = true;
        }
    }
}
