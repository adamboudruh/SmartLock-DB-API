
namespace SmartLock.DBApi.Models.Response
{
    public class ResponseKeyEntry
    {
        public Guid KeyId { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string TagUid { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
