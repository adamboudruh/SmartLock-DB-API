using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Response;
using SmartLock.DBApi.Operations;
using System.Net;

namespace SmartLock.DBApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IDevicesOperations _devicesOperations;

        public DevicesController(ILogger<DevicesController> logger, IDevicesOperations devicesOperations)
        {
            _logger = logger;
            _devicesOperations = devicesOperations;
        }

        // GET /devices/{deviceId}
        [HttpGet("{deviceId}")]
        [ProducesResponseType(typeof(Status<ResponseDevice>), 200)]
        [ProducesResponseType(typeof(Status<ResponseDevice>), 404)]
        public async Task<IActionResult> GetDevice(Guid deviceId)
        {
            var result = await _devicesOperations.GetDevice(deviceId);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result),
                HttpStatusCode.NotFound => NotFound(result),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }

        // GET /devices/{deviceId}/secret
        [HttpGet("{deviceId}/secret")]
        [ProducesResponseType(typeof(Status<string>), 200)]
        [ProducesResponseType(typeof(Status<string>), 404)]
        public async Task<IActionResult> GetDeviceSecret(Guid deviceId)
        {
            var result = await _devicesOperations.GetDeviceSecret(deviceId);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result),
                HttpStatusCode.NotFound => NotFound(result),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }
    }
}