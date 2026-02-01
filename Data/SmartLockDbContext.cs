using Microsoft.EntityFrameworkCore;
using SmartLock.DBApi.DataAccess;

namespace SmartLock.DBApi.Data
{
    public class SmartLockDbContext : DbContext
    {
        public SmartLockDbContext(DbContextOptions<SmartLockDbContext> options) : base(options)
        {
        }

        public DbSet<KeyEntry> Keys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KeyEntry>(entity =>
            {
                entity.HasKey(e => e.KeyId);
                entity.Property(e => e.TagUid).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.IsValid).HasDefaultValue(true);
            });
        }
    }
}