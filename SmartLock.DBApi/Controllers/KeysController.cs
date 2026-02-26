using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.DataAccess;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Models.Response;
using SmartLock.DBApi.Operations;
using System.Net;

namespace SmartLock.DBApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeysController : ControllerBase
    {
        private readonly ILogger<KeysController> _logger;
        private readonly IKeysOperations _keysOperations;

        public KeysController(ILogger<KeysController> logger, IKeysOperations keysOperations)
        {
            _logger = logger;
            _keysOperations = keysOperations;
        }

        // GET /keys
        [HttpGet]
        public async Task<IActionResult> GetAllKeys()
        {
            _logger.LogInformation("Getting all keys");
            var result = await _keysOperations.GetAllKeyEntries();

            return result.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(result),
                _ => new StatusCodeResult((int)result.StatusCode)
            };
        }

        // POST /keys
        // body: { name, tagUid }
        [HttpPost]
        [ProducesResponseType(typeof(Status<ResponseInsertKeyEntry>), 201)]
        [ProducesResponseType(typeof(Status<ResponseInsertKeyEntry>), 400)]
        public async Task<IActionResult> InsertKey([FromBody] InsertKeyEntry insertKeyEntry)
        {
            _logger.LogInformation("Creating new key entry");
            var result = await _keysOperations.InsertKeyEntry(insertKeyEntry);
            return result.StatusCode switch
            {
                HttpStatusCode.Created => CreatedAtAction(nameof(InsertKey), new { id = result.Data?.KeyId }, result),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(result),
                _ => new StatusCodeResult((int)result.StatusCode)
            };
        }

        // DELETE /keys/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteKey(int id)
        {
            _logger.LogInformation("Deleting key with id {Id}", id);
            var result = await _keysOperations.DeleteKeyEntry(new Guid(id.ToString()));
            return result.StatusCode switch
            {
                HttpStatusCode.NoContent => new NoContentResult(),
                HttpStatusCode.NotFound => new NotFoundObjectResult(result),
                _ => new StatusCodeResult((int)result.StatusCode)
            };
        }

        // GET /keys/test
        [HttpGet("test")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult GetTest()
        {
            _logger.LogInformation("Testing database connection by fetching all keys");
            return new OkObjectResult("Test successful, consider this endpoint reached!");
        }
    }
}
