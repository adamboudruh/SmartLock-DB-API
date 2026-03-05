using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.Models;
using SmartLock.DBApi.Models.Request;
using SmartLock.DBApi.Models.Response;
using System.Formats.Asn1;
using System.Net;

namespace SmartLock.DBApi.Operations
{
    public interface IDevicesOperations
    {
        Task<Status<ResponseDevice>> GetDevice(Guid deviceId);
        Task<Status<string>> GetDeviceSecret(Guid deviceId);
    }
    public class DevicesOperations : IDevicesOperations
    {
        private readonly SmartLockDbContext _db;
        private readonly ILogger<DevicesOperations> _logger;

        public DevicesOperations(SmartLockDbContext db, ILogger<DevicesOperations> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Status<ResponseDevice>> GetDevice(Guid deviceId)
        {
            _logger.LogInformation("Fetching device {DeviceId}.", deviceId);

            var device = await _db.Devices
                .Where(d => d.DeviceId == deviceId)
                .FirstOrDefaultAsync();

            if (device == null)
            {
                _logger.LogWarning("GetDevice failed: Device {DeviceId} not found.", deviceId);
                return new Status<ResponseDevice>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    StatusDetails = new List<string> { "Device not found." }
                };
            }

            return new Status<ResponseDevice>
            {
                StatusCode = HttpStatusCode.OK,
                Data = new ResponseDevice
                {
                    DeviceId = device.DeviceId,
                    Name = device.Name,
                    CreatedAt = device.CreatedAt
                }
            };
        }

        public async Task<Status<string>> GetDeviceSecret(Guid deviceId)
        {
            _logger.LogInformation("Fetching secret for device {DeviceId}.", deviceId);

            var device = await _db.Devices
                .Where(d => d.DeviceId == deviceId)
                .Select(d => new { d.DeviceSecret })
                .FirstOrDefaultAsync();

            if (device == null)
            {
                _logger.LogWarning("GetDeviceSecret failed: Device {DeviceId} not found.", deviceId);
                return new Status<string>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    StatusDetails = new List<string> { "Device not found." }
                };
            }

            return new Status<string>
            {
                StatusCode = HttpStatusCode.OK,
                Data = Convert.ToBase64String(device.DeviceSecret)
            };
        }
    }
}
