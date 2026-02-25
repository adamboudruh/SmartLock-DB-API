using System.ComponentModel.DataAnnotations;

namespace SmartLock.DBApi.DataAccess
{
    public class RfidKeyEntry
    {
        [Key]
        [Required]
        public Guid KeyId { get; set; } = Guid.NewGuid();

        public string? Name { get; set; }

        [Required]
        public string TagUid { get; set; } = null!;

        [Required]
        public bool IsValid { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUsed { get; set; }

        public ICollection<Event>? Events { get; set; }
    }
}