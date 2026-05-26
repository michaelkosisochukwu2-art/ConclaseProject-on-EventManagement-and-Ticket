using System.Reflection.Emit;
using ConclaseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ConclaseProject.Data
{
    public class EventDbContext:DbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
        {
        }

        public DbSet<Event> Events => Set<Event>();
        public DbSet<Attendees> Attendees => Set<Attendees>();
        public DbSet<EventPass> EventPasses => Set<EventPass>();
        public DbSet<VerificationLog> VerificationLogs => Set<VerificationLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Attendee Constraints
            modelBuilder.Entity<Attendees>()
                .HasIndex(a => a.Email)
                .IsUnique(); // Ensure unique emails globally

            // EventPass Constraints & Safeguards
            modelBuilder.Entity<EventPass>()
                .HasIndex(ep => new { ep.EventId, ep.AttendeeId })
                .IsUnique(); // Strict protection against duplicate event registration

            modelBuilder.Entity<EventPass>()
                .HasIndex(ep => ep.UniqueVerificationToken)
                .IsUnique(); // Ensure cryptographic token uniqueness

            // Concurrency Token Setup
            modelBuilder.Entity<EventPass>()
                .Property(ep => ep.RowVersion)
                .IsRowVersion(); // Protects database from simultaneous scans on multiple gate devices
        

        // Configured Entity Relationships
        modelBuilder.Entity<EventPass>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.EventPasses)
                .HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventPass>()
                .HasOne(ep => ep.Attendee)
                .WithMany(a => a.EventPasses)
                .HasForeignKey(ep => ep.AttendeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerificationLog>()
                .HasOne(vl => vl.EventPass)
                .WithMany(ep => ep.VerificationLogs)
                .HasForeignKey(vl => vl.EventPassId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
