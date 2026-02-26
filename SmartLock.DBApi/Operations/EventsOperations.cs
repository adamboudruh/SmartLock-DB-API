using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.DataAccess;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Enums;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Models.Response;
using System.Formats.Asn1;

namespace SmartLock.DBApi.Operations
{
    public interface IEventsOperations 
    {
        Task<Status<ResponseInsertEvent>> InsertEvent(InsertEvent request); 
        Task<Status<List<ResponseEvent>>> GetAllEvents();
        Task<Status<object>> ClearEvents();
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
            _logger.LogInformation("uid: ", request.TagUID);

            // Find the key by TagUid
            var key = await _db.Keys.FirstOrDefaultAsync(k => k.TagUid == request.TagUID);

            var newEvent = new DataAccess.Event
            {
                EventTypeId = request.EventTypeId,
                DeviceId = request.DeviceId,
                CreatedAt = DateTime.UtcNow,
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
                .Include(e => e.Key)           // ← join key name if present
                .OrderByDescending(e => e.CreatedAt)
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

    }
}
