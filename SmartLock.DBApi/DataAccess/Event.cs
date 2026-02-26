using Azure.Identity;
using SmartLock.DBApi.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartLock.DBApi.DataAccess
{
    public class Event
    {
        [Key]
        public Guid EventId { get; set; } = Guid.NewGuid();

        [Required]
        public int EventTypeId { get; set; }

        public Guid? DeviceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? KeyId { get; set; }

        [ForeignKey(nameof(KeyId))]
        public RfidKeyEntry? Key { get; set; }

        [ForeignKey(nameof(EventTypeId))]
        public EventType? EventType { get; set; }

        [ForeignKey(nameof(DeviceId))]
        public Device? Device { get; set; }
    }
}