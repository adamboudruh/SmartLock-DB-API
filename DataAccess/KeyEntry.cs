using System.ComponentModel.DataAnnotations;

namespace SmartLock.DBApi.DataAccess
{
    public class RfidKeyEntry
    {
        public Guid KeyId { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string TagUid { get; set; } = null!;
        public bool IsValid { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}