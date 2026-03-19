using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.DataAccess;
using SmartLock.DBApi.Models.Enums;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Operations;
using System.Net;

namespace SmartLock.DBApi.Tests
{
    public class KeysOperationsTests
    {
        private SmartLockDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<SmartLockDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SmartLockDbContext(options);
        }

        private KeysOperations CreateSut(SmartLockDbContext db)
            => new KeysOperations(db, NullLogger<KeysOperations>.Instance);

        #region InsertKeyEntry Tests

        [Fact]
        public async Task InsertKeyEntry_ValidRequest_ReturnsCreated()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertKeyEntry(new InsertKeyEntry
            {
                Name = "Yellow Key",
                TagUid = "04AB0A613E6180"
            });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.NotEqual(Guid.Empty, result.Data.KeyId);
        }

        [Fact]
        public async Task InsertKeyEntry_DuplicateTagUid_ReturnsConflict()
        {
            var db = CreateDb();
            var existingKey = new RfidKeyEntry
            {
                KeyId = Guid.NewGuid(),
                TagUid = "04AB0A613E6180",
                Name = "Existing Key"
            };
            db.Keys.Add(existingKey);
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.InsertKeyEntry(new InsertKeyEntry
            {
                Name = "Duplicate Key",
                TagUid = "04AB0A613E6180"
            });

            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        }

        #endregion

        #region DeleteKeyEntry Tests

        [Fact]
        public async Task DeleteKeyEntry_ExistingKey_DeletesKeyAndNullifiesRelatedEvents()
        {
            var db = CreateDb();
            var key = new RfidKeyEntry
            {
                KeyId = Guid.NewGuid(),
                Name = "Key for Deletion",
                TagUid = "04DEADC0DE",
                CreatedAt = DateTime.UtcNow
            };
            db.Keys.Add(key);
            db.Events.Add(new Event
            {
                EventId = Guid.NewGuid(),
                EventTypeId = (int)EventTypes.SuccessKeyUnlock,
                KeyId = key.KeyId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.DeleteKeyEntry(key.KeyId);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(await db.Keys.FindAsync(key.KeyId)); // Key should be deleted
            Assert.Null((await db.Events.FirstAsync()).KeyId); // Event's KeyId should be null
        }

        [Fact]
        public async Task DeleteKeyEntry_NonExistentKey_ReturnsNotFound()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.DeleteKeyEntry(Guid.NewGuid());

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        #endregion
    }
}