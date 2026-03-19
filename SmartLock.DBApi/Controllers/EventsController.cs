using Microsoft.AspNetCore.Mvc;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Models.Response;
using SmartLock.DBApi.Operations;
using System.Net;

namespace SmartLock.DBApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IEventsOperations _eventsOperations;

        public EventsController(ILogger<EventsController> logger, IEventsOperations eventsOperations)
        {
            _logger = logger;
            _eventsOperations = eventsOperations;
        }

        // POST /events
        // body: { name, tagUid }
        [HttpPost]
        [ProducesResponseType(typeof(Status<ResponseInsertEvent>), 201)]
        [ProducesResponseType(typeof(Status<ResponseInsertEvent>), 400)]
        public async Task<IActionResult> InsertEvent([FromBody] InsertEvent insertEvent)
        {
            _logger.LogInformation("Registering event in database");
            var result = await _eventsOperations.InsertEvent(insertEvent);

            // Return the whole Status<T> payload as the response body so clients receive StatusDetails and Data.
            return result.StatusCode switch
            {
                HttpStatusCode.Created => StatusCode((int)result.StatusCode, result),
                HttpStatusCode.BadRequest => BadRequest(result),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }

        [HttpPost("bulk")]
        [ProducesResponseType(typeof(Status<int>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> BulkInsertEvents([FromBody] BulkInsertEvent request)
        {
            _logger.LogInformation("Bulk inserting {Count} offline events", request.Events.Count);

            var result = await _eventsOperations.BulkInsertEvents(request);
            return result.StatusCode switch
            {
                HttpStatusCode.Created => StatusCode(201, result),
                HttpStatusCode.BadRequest => BadRequest(result),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }

        // GET /events
        [HttpGet]
        [ProducesResponseType(typeof(Status<List<ResponseEvent>>), 200)]
        public async Task<IActionResult> GetAllEvents()
        {
            _logger.LogInformation("Fetching all events from database");
            var result = await _eventsOperations.GetAllEvents();
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }

        // DELETE /events
        [HttpDelete]
        [ProducesResponseType(204)]
        public async Task<IActionResult> ClearEvents()
        {
            _logger.LogInformation("Clearing all events from database");
            var result = await _eventsOperations.ClearEvents();
            return result.StatusCode switch
            {
                HttpStatusCode.NoContent => NoContent(),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }
    }
}