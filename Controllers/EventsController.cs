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
        [ProducesResponseType(typeof(Status<ResponseInsertEvent>), 202)]
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
    }
}
