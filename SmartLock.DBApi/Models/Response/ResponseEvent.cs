namespace SmartLock.DBApi.Models.Response
{
    public class ResponseEvent
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = null!; // enum name
        public Guid? DeviceId { get; set; }
        public Guid? KeyId { get; set; }
        public string? KeyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}