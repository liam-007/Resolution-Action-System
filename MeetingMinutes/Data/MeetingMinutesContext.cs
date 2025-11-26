using MeetingMinutes.Models;
using Microsoft.EntityFrameworkCore;
namespace MeetingMinutes.Data
{
    public class MeetingMinutesContext : DbContext
    {
        public MeetingMinutesContext(DbContextOptions<MeetingMinutesContext> options)
            : base(options)
        {
            
    }

        public DbSet<MeetingType> MeetingTypes { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingItem> MeetingItems { get; set; }
        public DbSet<MeetingItemStatus> MeetingItemStatuses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            // Map entities to singular table names in SQL
            modelBuilder.Entity<MeetingType>().ToTable("MeetingType");
            modelBuilder.Entity<Meeting>().ToTable("Meeting");
            modelBuilder.Entity<MeetingItem>().ToTable("MeetingItem");
            modelBuilder.Entity<MeetingItemStatus>().ToTable("MeetingItemStatus");

        // Explicitly set primary key for MeetingItemStatus
        modelBuilder.Entity<MeetingItemStatus>()
                .HasKey(s => s.StatusId);
        }

    }
}