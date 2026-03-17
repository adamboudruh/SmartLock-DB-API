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
                HttpStatusCode.OK => Ok(result),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }

        // POST /keys
        // body: { name, tagUid }
        [HttpPost]
        [ProducesResponseType(typeof(Status<ResponseInsertKeyEntry>), 201)]
        [ProducesResponseType(typeof(Status<ResponseInsertKeyEntry>), 400)]
        public async Task<IActionResult> Create([FromBody] InsertKeyEntry insertKeyEntry)
        {
            _logger.LogInformation("Creating new key entry");
            var result = await _keysOperations.InsertKeyEntry(insertKeyEntry);
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result),
                HttpStatusCode.Created => StatusCode((int)result.StatusCode, result),
                HttpStatusCode.BadRequest => BadRequest(result),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }

        // GET /keys/test
        [HttpGet("test")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult GetTest()
        {
            _logger.LogInformation("Testing database connection by fetching all keys");
            return Ok("Test successful, consider this endpoint reached!");
        }
    }
}