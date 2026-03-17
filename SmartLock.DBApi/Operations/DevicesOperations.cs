using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.DataAccess;
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
        Task<Status<List<ResponseDeviceSetting>>> GetDeviceSettings(Guid deviceId);
        Task<Status<List<ResponseDeviceSetting>>> UpdateDeviceSettings(Guid deviceId, UpdateDeviceSettings request);
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

        public async Task<Status<List<ResponseDeviceSetting>>> GetDeviceSettings(Guid deviceId)
        {
            _logger.LogInformation("Fetching settings for device {DeviceId}", deviceId);

            var device = await _db.Devices.FindAsync(deviceId);
            if (device == null)
            {
                return new Status<List<ResponseDeviceSetting>>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    StatusDetails = new List<string> { "Device not found." }
                };
            }

            var allSettings = await _db.Settings.ToListAsync();

            var deviceSettings = await _db.DeviceSettings
                .Where(ds => ds.DeviceId == deviceId)
                .ToListAsync();

            // Merge: use device value if exists, otherwise default
            var result = allSettings.Select(s =>
            {
                var deviceSetting = deviceSettings.FirstOrDefault(ds => ds.SettingId == s.SettingId);
                return new ResponseDeviceSetting
                {
                    SettingId = s.SettingId,
                    Name = s.Name,
                    Value = deviceSetting?.Value ?? s.DefaultValue,
                    DefaultValue = s.DefaultValue
                };
            }).ToList();

            return new Status<List<ResponseDeviceSetting>>
            {
                StatusCode = HttpStatusCode.OK,
                Data = result
            };
        }

        public async Task<Status<List<ResponseDeviceSetting>>> UpdateDeviceSettings(Guid deviceId, UpdateDeviceSettings request)
        {
            _logger.LogInformation("Updating {Count} settings for device {DeviceId}",
                request.Settings.Count, deviceId);

            var device = await _db.Devices.FindAsync(deviceId);
            if (device == null)
            {
                return new Status<List<ResponseDeviceSetting>>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    StatusDetails = new List<string> { "Device not found." }
                };
            }

            // validate all setting IDs exist
            var validSettingIdsList = await _db.Settings.Select(s => s.SettingId).ToListAsync();
            var validSettingIds = new HashSet<int>(validSettingIdsList); // for fast lookup

            for (int i = 0; i < request.Settings.Count; i++)
            {
                var settingId = request.Settings[i].SettingId;
                if (!validSettingIds.Contains(settingId))
                {
                    return new Status<List<ResponseDeviceSetting>>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        StatusDetails = new List<string> { $"Invalid SettingId: {settingId}" }
                    };
                }
            }

            // Upsert each setting
            foreach (var entry in request.Settings)
            {
                var existing = await _db.DeviceSettings
                    .FirstOrDefaultAsync(ds => ds.DeviceId == deviceId && ds.SettingId == entry.SettingId);

                if (existing != null)
                {
                    existing.Value = entry.Value; // replace the value
                }
                else
                {
                    _db.DeviceSettings.Add(new DeviceSetting
                    {
                        DeviceSettingId = Guid.NewGuid(),
                        DeviceId = deviceId,
                        SettingId = entry.SettingId,
                        Value = entry.Value
                    });
                }
            }

            await _db.SaveChangesAsync();
            return await GetDeviceSettings(deviceId);
        }
    }
}
