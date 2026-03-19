using Microsoft.EntityFrameworkCore;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Enums;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Models.Response;

namespace SmartLock.DBApi.Operations
{
    public interface IEventsOperations 
    {
        Task<Status<ResponseInsertEvent>> InsertEvent(InsertEvent request); 
        Task<Status<List<ResponseEvent>>> GetAllEvents();
        Task<Status<object>> ClearEvents();
        Task<Status<int>> BulkInsertEvents(BulkInsertEvent request);
    }
    public class EventsOperations : IEventsOperations
    {
        private readonly SmartLockDbContext _db;
        private readonly ILogger<EventsOperations> _logger;

        public EventsOperations(SmartLockDbContext db, ILogger<EventsOperations> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Status<ResponseInsertEvent>> InsertEvent(InsertEvent request)
        {
            _logger.LogInformation("Inserting new event into database.");
            // Ensure the EventType exists  --WRONG, just check in enums
            if (!Enum.IsDefined(typeof(EventTypes), request.EventTypeId))
            {
                _logger.LogWarning("InsertEvent failed: Invalid EventTypeId.");
                return new Status<ResponseInsertEvent>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    StatusDetails = new List<string> { "Invalid EventTypeId." },
                    Data = null
                };
            }

            // Find the key by TagUid
            var key = request.TagUid != null
                    ? await _db.Keys.FirstOrDefaultAsync(k => k.TagUid == request.TagUid)
                    : null;

            var newEvent = new DataAccess.Event
            {
                EventTypeId = request.EventTypeId,
                DeviceId = request.DeviceId,
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                KeyId = key?.KeyId
            };

            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();
            var responseData = new ResponseInsertEvent
            {
                EventId = newEvent.EventId,
            };
            return new Status<ResponseInsertEvent>
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                Data = responseData
            };
        }

        public async Task<Status<List<ResponseEvent>>> GetAllEvents()
        {
            _logger.LogInformation("Fetching all events from database.");
            var events = await _db.Events
                .Include(e => e.Key)           // join key name if present
                .OrderByDescending(e => e.CreatedAt)
                .Take(200) // grab 200 latest events to prevent overload, pagination in future
                .ToListAsync();

            return new Status<List<ResponseEvent>>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Data = events.Select(e => new ResponseEvent
                {
                    EventId = e.EventId,
                    EventType = Enum.GetName(typeof(EventTypes), e.EventTypeId) ?? e.EventTypeId.ToString(),
                    DeviceId = e.DeviceId,
                    KeyId = e.KeyId,
                    KeyName = e.Key?.Name,  // ← null if no key associated
                    CreatedAt = e.CreatedAt
                }).ToList()
            };
        }

        public async Task<Status<object>> ClearEvents()
        {
            _logger.LogInformation("Clearing all events from database.");
            await _db.Events.ExecuteDeleteAsync(); // ← EF Core 7+ bulk delete, no load needed
            return new Status<object>
            {
                StatusCode = System.Net.HttpStatusCode.NoContent,
                Data = null
            };
        }

        public async Task<Status<int>> BulkInsertEvents(BulkInsertEvent request)
        {
            if (request.Events == null || request.Events.Count == 0)
            {
                return new Status<int>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    StatusDetails = new List<string> { "No events provided." }
                };
            }

            _logger.LogInformation("Bulk inserting {Count} offline events.", request.Events.Count);

            var newEvents = new List<DataAccess.Event>();

            foreach (var e in request.Events)
            {
                if (!Enum.IsDefined(typeof(EventTypes), e.EventTypeId))
                {
                    _logger.LogWarning("InsertEvent failed: Invalid EventTypeId.");
                    continue;
                }

                var key = e.TagUid != null
                    ? await _db.Keys.FirstOrDefaultAsync(k => k.TagUid == e.TagUid)
                    : null;

                newEvents.Add(new DataAccess.Event
                {
                    EventTypeId = e.EventTypeId,
                    DeviceId = e.DeviceId,
                    KeyId = key?.KeyId,
                    CreatedAt = e.CreatedAt ?? DateTime.UtcNow
                });
            }

            _db.Events.AddRange(newEvents);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Bulk inserted {Count} events.", newEvents.Count);

            return new Status<int>
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                Data = newEvents.Count
            };
        }
    }
}
