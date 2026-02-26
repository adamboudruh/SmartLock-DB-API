using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.DataAccess;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Operations;
using System.Net;
using Microsoft.EntityFrameworkCore.InMemory;

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

        #region GetAllKeyEntries

        [Fact]
        public async Task GetAllKeyEntries_NoKeys_ReturnsEmptyList()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.GetAllKeyEntries();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllKeyEntries_ReturnsKeysOrderedByCreatedAt()
        {
            var db = CreateDb();
            db.Keys.AddRange(
                new RfidKeyEntry { KeyId = Guid.NewGuid(), Name = "Red Key", TagUid = "04C9B7603E6180", IsValid = true, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
                new RfidKeyEntry { KeyId = Guid.NewGuid(), Name = "Yellow Key", TagUid = "04AB0A613E6180", IsValid = true, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.GetAllKeyEntries();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Red Key", result.Data[0].Name); // ← oldest first
        }

        [Fact]
        public async Task GetAllKeyEntries_MapsFieldsCorrectly()
        {
            var db = CreateDb();
            var key = new RfidKeyEntry
            {
                KeyId = Guid.NewGuid(),
                Name = "Green Key",
                TagUid = "046AF0603E6180",
                IsValid = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Keys.Add(key);
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.GetAllKeyEntries();

            var entry = result.Data[0];
            Assert.Equal(key.KeyId, entry.KeyId);
            Assert.Equal(key.Name, entry.Name);
            Assert.Equal(key.TagUid, entry.TagUid);
            Assert.Equal(key.CreatedAt, entry.CreatedAt);
        }

        #endregion

        #region InsertKeyEntry

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
        public async Task InsertKeyEntry_NullName_ReturnsBadRequest()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertKeyEntry(new InsertKeyEntry { TagUid = "04AB0A613E6180" });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Contains("Name is required.", result.StatusDetails);
        }

        [Fact]
        public async Task InsertKeyEntry_NullTagUid_ReturnsBadRequest()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertKeyEntry(new InsertKeyEntry { Name = "Yellow Key" });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Contains("TagUid is required.", result.StatusDetails);
        }

        [Fact]
        public async Task InsertKeyEntry_ValidRequest_PersistsToDatabase()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            await sut.InsertKeyEntry(new InsertKeyEntry
            {
                Name = "Red Key",
                TagUid = "04C9B7603E6180"
            });

            var saved = await db.Keys.FirstAsync();
            Assert.Equal("Red Key", saved.Name);
            Assert.Equal("04C9B7603E6180", saved.TagUid);
            Assert.True(saved.IsValid);
            Assert.NotEqual(Guid.Empty, saved.KeyId);
        }

        #endregion

        #region DeleteKeyEntry

        [Fact]
        public async Task DeleteKeyEntry_ExistingKey_ReturnsNoContent()
        {
            var db = CreateDb();
            var key = new RfidKeyEntry
            {
                KeyId = Guid.NewGuid(),
                Name = "Yellow Key",
                TagUid = "04AB0A613E6180",
                IsValid = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Keys.Add(key);
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.DeleteKeyEntry(key.KeyId);

            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            Assert.Equal(0, await db.Keys.CountAsync());
        }

        [Fact]
        public async Task DeleteKeyEntry_NonExistentKey_ReturnsNotFound()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.DeleteKeyEntry(Guid.NewGuid());

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(result.StatusDetails);
        }

        [Fact]
        public async Task DeleteKeyEntry_OnlyDeletesTargetKey()
        {
            var db = CreateDb();
            var keyToDelete = new RfidKeyEntry { KeyId = Guid.NewGuid(), Name = "Red Key", TagUid = "04C9B7603E6180", IsValid = true, CreatedAt = DateTime.UtcNow };
            var keyToKeep = new RfidKeyEntry { KeyId = Guid.NewGuid(), Name = "Yellow Key", TagUid = "04AB0A613E6180", IsValid = true, CreatedAt = DateTime.UtcNow };
            db.Keys.AddRange(keyToDelete, keyToKeep);
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            await sut.DeleteKeyEntry(keyToDelete.KeyId);

            Assert.Equal(1, await db.Keys.CountAsync());
            Assert.NotNull(await db.Keys.FindAsync(keyToKeep.KeyId));
        }

        #endregion
    }
}