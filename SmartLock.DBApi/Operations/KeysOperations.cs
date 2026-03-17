using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Models.Response;
using System.Drawing;
using System.Formats.Asn1;
using System.Net;

namespace SmartLock.DBApi.Operations
{
    public interface IKeysOperations
    {
        Task<Status<List<ResponseKeyEntry>>> GetAllKeyEntries();
        Task<Status<ResponseInsertKeyEntry>> InsertKeyEntry(InsertKeyEntry insertKeyEntry);
        Task<Status<string>> DeleteKeyEntry(Guid id);
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
                    Color = k.Color,
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

            var duplicateKey = await _db.Keys.FirstOrDefaultAsync(k => k.TagUid == insertKeyEntry.TagUid);
            if (duplicateKey != null)
            {
                _logger.LogWarning("InsertKeyEntry failed: Key with UID already exists (name: {Name}).", duplicateKey.Name);
                return new Status<ResponseInsertKeyEntry>
                {
                    StatusCode = HttpStatusCode.Conflict,
                    StatusDetails = new List<string> { $"A key with this UID already exists (registered as \"{duplicateKey.Name}\")." }
                };
            }

            var newKeyEntry = new DataAccess.RfidKeyEntry
            {
                Name = insertKeyEntry.Name,
                TagUid = insertKeyEntry.TagUid,
                Color = insertKeyEntry.Color,
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

        public async Task<Status<string>> DeleteKeyEntry(Guid id)
        {
            var key = await _db.Keys.FindAsync(id);
            if (key == null)
            {
                return new Status<string>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    StatusDetails = new List<string> { "Key not found." }
                };
            }

            // nullify the KeyId on all events referencing this key
            var relatedEvents = await _db.Events
                .Where(e => e.KeyId == id)
                .ToListAsync();

            foreach (var ev in relatedEvents)
            {
                ev.KeyId = null;
            }

            _db.Keys.Remove(key);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted key {KeyId}, nullified {Count} related events.", id, relatedEvents.Count);

            return new Status<string>
            {
                StatusCode = HttpStatusCode.OK,
                Data = $"Key deleted. {relatedEvents.Count} events updated."
            };
        }
    }
}
