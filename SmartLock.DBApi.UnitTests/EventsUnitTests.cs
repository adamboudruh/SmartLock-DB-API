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

        #region InsertEvent Tests

        [Fact]
        public async Task InsertEvent_ValidRequest_ReturnsCreated()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertEvent(new InsertEvent
            {
                EventTypeId = (int)EventTypes.RemoteLock,
                CreatedAt = DateTime.UtcNow
            });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task InsertEvent_InvalidEventTypeId_ReturnsBadRequest()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertEvent(new InsertEvent { EventTypeId = 999 });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task InsertEvent_NullCreatedAt_UsesCurrentUTC()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.InsertEvent(new InsertEvent
            {
                EventTypeId = (int)EventTypes.ButtonUnlock,
                CreatedAt = null
            });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            var insertedEvent = await db.Events.FirstOrDefaultAsync();
            Assert.NotNull(insertedEvent);
            Assert.True(insertedEvent.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        #endregion

        #region BulkInsertEvents Tests

        [Fact]
        public async Task BulkInsertEvents_ValidMultipleEvents_ReturnsCreatedCount()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.BulkInsertEvents(new BulkInsertEvent
            {
                Events = new List<InsertEvent>
                {
                    new InsertEvent { EventTypeId = (int)EventTypes.RemoteLock, CreatedAt = DateTime.UtcNow },
                    new InsertEvent { EventTypeId = (int)EventTypes.ButtonUnlock, CreatedAt = DateTime.UtcNow }
                }
            });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal(2, result.Data);
            Assert.Equal(2, await db.Events.CountAsync());
        }

        [Fact]
        public async Task BulkInsertEvents_SomeInvalidEvents_SkipsInvalidEvents()
        {
            var db = CreateDb();
            var sut = CreateSut(db);

            var result = await sut.BulkInsertEvents(new BulkInsertEvent
            {
                Events = new List<InsertEvent>
                {
                    new InsertEvent { EventTypeId = (int)EventTypes.RemoteLock, CreatedAt = DateTime.UtcNow },
                    new InsertEvent { EventTypeId = 999, CreatedAt = DateTime.UtcNow } // Invalid EventType
                }
            });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal(1, result.Data);
            Assert.Equal(1, await db.Events.CountAsync());
        }

        #endregion

        #region GetAllEvents Tests

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

        #endregion
    }
}