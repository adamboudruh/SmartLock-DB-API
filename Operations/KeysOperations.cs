using System.Formats.Asn1;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Response;
using SmartLock.DBApi.Models.Request;

namespace SmartLock.DBApi.Operations
{
    public interface IKeysOperations
    {
        Task<Status<List<ResponseKeyEntry>>> GetAllKeyEntries();
        Task<Status<ResponseInsertKeyEntry>> InsertKeyEntry(InsertKeyEntry insertKeyEntry);
        Task<Status<object>> DeleteKeyEntry(Guid id);
    }
    public class KeysOperations : IKeysOperations
    {
        private readonly SmartLockDbContext _db;
        private readonly ILogger<KeysOperations> _logger;

        public KeysOperations(SmartLockDbContext db, ILogger<KeysOperations> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Status<List<ResponseKeyEntry>>> GetAllKeyEntries()
        {
            _logger.LogInformation("Fetching all keys from database.");
            var allKeys = await _db.Keys.OrderBy(k => k.CreatedAt).ToListAsync();

            var result = allKeys;

            return new Status<List<ResponseKeyEntry>>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Data = result.Select(k => new ResponseKeyEntry
                {
                    KeyId = k.KeyId,
                    Name = k.Name,
                    TagUid = k.TagUid,
                    CreatedAt = k.CreatedAt
                }).ToList()
            };
        }

        public async Task<Status<ResponseInsertKeyEntry>> InsertKeyEntry(InsertKeyEntry insertKeyEntry)
        {
            _logger.LogInformation("Inserting new key entry into database.");
            if (insertKeyEntry.Name == null)
            {
                _logger.LogWarning("InsertKeyEntry failed: Name is null.");
                return new Status<ResponseInsertKeyEntry>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    StatusDetails = new List<string> { "Name is required." },
                    Data = null
                };
            }

            if (insertKeyEntry.TagUid == null)
            {
                _logger.LogWarning("InsertKeyEntry failed: TagUid is null.");
                return new Status<ResponseInsertKeyEntry>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    StatusDetails = new List<string> { "TagUid is required." },
                    Data = null
                };
            }

            var newKeyEntry = new DataAccess.RfidKeyEntry
            {
                Name = insertKeyEntry.Name,
                TagUid = insertKeyEntry.TagUid,
                IsValid = true,
                KeyId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            _db.Keys.Add(newKeyEntry);
            await _db.SaveChangesAsync();
            var responseData = new ResponseInsertKeyEntry
            {
                KeyId = newKeyEntry.KeyId,
            };
            return new Status<ResponseInsertKeyEntry>
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                Data = responseData
            };
        }

        public async Task<Status<object>> DeleteKeyEntry(Guid id)
        {
            _logger.LogInformation("Deleting key entry with id {Id}", id);
            var key = await _db.Keys.FindAsync(id);
            if (key == null)
            {
                _logger.LogWarning("DeleteKeyEntry failed: Key with id {Id} not found.", id);
                return new Status<object>
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    StatusDetails = new List<string> { $"Key with id {id} not found." },
                    Data = null
                };
            }

            _db.Keys.Remove(key);
            await _db.SaveChangesAsync();

            return new Status<object>
            {
                StatusCode = System.Net.HttpStatusCode.NoContent,
                Data = null
            };
        }
    }
}
