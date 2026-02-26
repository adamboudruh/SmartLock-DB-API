using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.DataAccess;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Operations;
using SmartLock.DBApi.Models.Request;
using System.Net;
using SmartLock.DBApi.Models.Response;

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
            return result.StatusCode switch
            {
                HttpStatusCode.Created => CreatedAtAction(nameof(InsertEvent), new { id = result.Data?.EventId }, result),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(result),
                _ => new StatusCodeResult((int)result.StatusCode)
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
                HttpStatusCode.OK => new OkObjectResult(result),
                _ => new StatusCodeResult((int)result.StatusCode)
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
                HttpStatusCode.NoContent => new NoContentResult(),
                _ => new StatusCodeResult((int)result.StatusCode)
            };
        }
    }
}
