namespace SmartLock.DBApi.Models.Response
{
    public class ResponseDevice
    {
        public Guid DeviceId { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}