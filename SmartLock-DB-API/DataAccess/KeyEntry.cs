using System.ComponentModel.DataAnnotations;

namespace SmartLock.DBApi.DataAccess
{
    public class KeyEntry
    {
        [Key]
        public Guid KeyId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(64)]
        public string TagUid { get; set; } = null!;

        public bool IsValid { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}