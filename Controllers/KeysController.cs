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
    public class KeysController
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
        [ProducesResponseType(typeof(Status<ResponseInsertKeyEntry>), 200)]
        [ProducesResponseType(typeof(Status<ResponseInsertKeyEntry>), 400)]
        public async Task<IActionResult> Create([FromBody] InsertKeyEntry insertKeyEntry)
        {
            _logger.LogInformation("Creating new key entry");
            var result = await _keysOperations.InsertKeyEntry(insertKeyEntry);
            return result.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(result),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(result),
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
