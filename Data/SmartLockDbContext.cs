using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartLock.DBApi.DataAccess;
using SmartLock.DBApi.Models.Enums;

namespace SmartLock.DBApi.Data
{
    public class SmartLockDbContext : DbContext
    {
        public SmartLockDbContext(DbContextOptions<SmartLockDbContext> options) : base(options)
        {
        }

        public DbSet<RfidKeyEntry> Keys { get; set; } = null!;
        public DbSet<EventType> EventTypes { get; set; } = null!;
        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RfidKeyEntry
            modelBuilder.Entity<RfidKeyEntry>(entity =>
            {
                entity.HasKey(e => e.KeyId);
                entity.Property(e => e.TagUid).IsRequired().HasMaxLength(64);
                entity.HasIndex(e => e.TagUid).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.IsValid).HasDefaultValue(true);
            });

            // EventType
            modelBuilder.Entity<EventType>(entity =>
            {
                entity.HasKey(e => e.EventTypeId);
                entity.Property(e => e.EventTypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

            // Seed EventType values from enum
            var eventTypeSeed = Enum.GetValues(typeof(EventTypes))
                                    .Cast<EventTypes>()
                                    .Select(e => new EventType
                                    {
                                        EventTypeId = (int)e,
                                        Name = e.ToString()
                                    })
                                    .ToArray();
            modelBuilder.Entity<EventType>().HasData(eventTypeSeed);

            // Device
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(d => d.DeviceId);
                entity.Property(d => d.Name).HasMaxLength(200);
                entity.Property(d => d.DeviceSecret).IsRequired();
                entity.Property(d => d.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.EventType)
                      .WithMany(et => et.Events)
                      .HasForeignKey(e => e.EventTypeId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();

                entity.HasOne(e => e.Device)
                      .WithMany(d => d.Events)
                      .HasForeignKey(e => e.DeviceId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();

                entity.HasIndex(e => new { e.DeviceId, e.EventTypeId });
            });
        }
    }
}