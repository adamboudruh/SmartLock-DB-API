using SmartLock.DBApi.Utils;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace SmartLock.DBApi.Models.Request
{
    public class InsertEvent
    {
        [Required]
        public int EventTypeId { get; set; } // E.g., 1 = ButtonLock, 2 = RemoteLock, etc.

        public Guid? DeviceId { get; set; } // Device GUID

        public String? TagUid { get; set; } // RFID tag UID, if applicable

        private DateTime? _createdAt;

        [JsonConverter(typeof(NullableDateTimeJsonConverter))]
        public DateTime? CreatedAt
        {
            get => _createdAt;
            set
            {
                if (value.HasValue)
                    _createdAt = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                else
                    _createdAt = null;
            }
        }

        public void ParseCreatedAt(string input)
        {
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime parsedUtc))
            {
                CreatedAt = DateTime.SpecifyKind(parsedUtc, DateTimeKind.Utc);
            }
            else
            {
                throw new FormatException($"Invalid timestamp: {input}");
            }
        }
    }
}