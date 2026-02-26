using Azure.Identity;
using System.ComponentModel.DataAnnotations;
using SmartLock.DBApi.Models.Enums;

namespace SmartLock.DBApi.DataAccess
{
    public class Device
    {
        [Key]
        public Guid DeviceId { get; set; } = Guid.NewGuid();

        [MaxLength(200)]
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public byte[] DeviceSecret { get; set; } = Array.Empty<byte>();

        public ICollection<Event>? Events { get; set; }
    }
}