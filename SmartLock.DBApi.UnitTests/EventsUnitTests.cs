using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.DataAccess;
using SmartLock.DBApi.Models.Enums;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Operations;
using System.Net;
using Microsoft.EntityFrameworkCore.InMemory;

namespace SmartLock.DBApi.Tests
{
    public class EventsOperationsTests
    {
        private SmartLockDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<SmartLockDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SmartLockDbContext(options);
        }

        private EventsOperations CreateSut(SmartLockDbContext db)
            => new EventsOperations(db, NullLogger<EventsOperations>.Instance);

        #region InsertEvent

        [Fact]
        public async Task InsertEvent_ValidRequest_ReturnsCreated()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertEvent(new InsertEvent { EventTypeId = (int)EventTypes.RemoteLock });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.NotEqual(Guid.Empty, result.Data.EventId);
        }

        [Fact]
        public async Task InsertEvent_InvalidEventTypeId_ReturnsBadRequest()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertEvent(new InsertEvent { EventTypeId = 999 });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Contains("Invalid EventTypeId.", result.StatusDetails);
        }

        [Fact]
        public async Task InsertEvent_WithMatchingTagUid_LinksKeyToEvent()
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
            var result = await sut.InsertEvent(new InsertEvent
            {
                EventTypeId = (int)EventTypes.SuccessKeyUnlock,
                TagUID = "04AB0A613E6180"
            });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            var inserted = await db.Events.FirstAsync();
            Assert.Equal(key.KeyId, inserted.KeyId);
        }

        [Fact]
        public async Task InsertEvent_WithUnknownTagUid_InsertsEventWithNullKeyId()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertEvent(new InsertEvent
            {
                EventTypeId = (int)EventTypes.FailKeyUnlock,
                TagUID = "DOESNOTEXIST"
            });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            var inserted = await db.Events.FirstAsync();
            Assert.Null(inserted.KeyId);
        }

        #endregion

        #region GetAllEvents

        [Fact]
        public async Task GetAllEvents_NoEvents_ReturnsEmptyList()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.GetAllEvents();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsEventsOrderedByCreatedAtDescending()
        {
            var db = CreateDb();
            db.Events.AddRange(
                new Event { EventId = Guid.NewGuid(), EventTypeId = (int)EventTypes.ButtonLock, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                new Event { EventId = Guid.NewGuid(), EventTypeId = (int)EventTypes.RemoteUnlock, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.GetAllEvents();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(2, result.Data.Count);
            Assert.True(result.Data[0].CreatedAt > result.Data[1].CreatedAt);
        }

        [Fact]
        public async Task GetAllEvents_MapsEventTypeIdToName()
        {
            var db = CreateDb();
            db.Events.Add(new Event
            {
                EventId = Guid.NewGuid(),
                EventTypeId = (int)EventTypes.RemoteLock,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.GetAllEvents();

            Assert.Equal("RemoteLock", result.Data[0].EventType);
        }

        [Fact]
        public async Task GetAllEvents_IncludesKeyName_WhenKeyIsLinked()
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
            db.Events.Add(new Event
            {
                EventId = Guid.NewGuid(),
                EventTypeId = (int)EventTypes.SuccessKeyUnlock,
                KeyId = key.KeyId,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.GetAllEvents();

            Assert.Equal("Green Key", result.Data[0].KeyName);
        }

        #endregion

        #region ClearEvents

        [Fact]
        public async Task ClearEvents_DeletesAllEvents_ReturnsNoContent()
        {
            var db = CreateDb();
            db.Events.AddRange(
                new Event { EventId = Guid.NewGuid(), EventTypeId = (int)EventTypes.Open, CreatedAt = DateTime.UtcNow },
                new Event { EventId = Guid.NewGuid(), EventTypeId = (int)EventTypes.Close, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var sut = CreateSut(db);
            var result = await sut.ClearEvents();

            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            Assert.Equal(0, await db.Events.CountAsync());
        }

        [Fact]
        public async Task ClearEvents_EmptyTable_StillReturnsNoContent()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.ClearEvents();

            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        #endregion
    }
}