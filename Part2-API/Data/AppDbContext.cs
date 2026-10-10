using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------- Roles ----------
            modelBuilder.Entity<Role>(e =>
            {
                e.Property(r => r.RoleName).HasMaxLength(20).IsUnicode(false);
                e.HasIndex(r => r.RoleName).IsUnique();

                // Both roles must exist before anyone can register
                e.HasData(
                    new Role { RoleID = 1, RoleName = "Organiser" },
                    new Role { RoleID = 2, RoleName = "Participant" });
            });

            // ---------- Users ----------
            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.FullName).HasMaxLength(100).IsUnicode(false);
                e.Property(u => u.Email).HasMaxLength(150).IsUnicode(false);
                e.Property(u => u.PasswordHash).HasMaxLength(255).IsUnicode(false);
                e.Property(u => u.DateRegistered).HasDefaultValueSql("GETDATE()");
                e.HasIndex(u => u.Email).IsUnique();

                e.HasOne(u => u.Role)
                 .WithMany(r => r.Users)
                 .HasForeignKey(u => u.RoleID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Events ----------
            modelBuilder.Entity<Event>(e =>
            {
                e.Property(x => x.EventName).HasMaxLength(150).IsUnicode(false);
                e.Property(x => x.Description).HasMaxLength(500).IsUnicode(false);
                e.Property(x => x.Location).HasMaxLength(150).IsUnicode(false);
                e.Property(x => x.Distance).HasColumnType("decimal(5,2)");
                e.Property(x => x.EventType).HasMaxLength(10).IsUnicode(false);
                e.Property(x => x.DateCreated).HasDefaultValueSql("GETDATE()");

                e.ToTable(t => t.HasCheckConstraint(
                    "CK_Events_EventType", "[EventType] IN ('Run', 'Walk', 'Cycle')"));

                e.HasOne(x => x.Organiser)
                 .WithMany(u => u.Events)
                 .HasForeignKey(x => x.OrganiserID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Categories ----------
            modelBuilder.Entity<Category>(e =>
            {
                e.Property(c => c.CategoryName).HasMaxLength(50).IsUnicode(false);
                e.Property(c => c.CategoryType).HasMaxLength(10).IsUnicode(false);

                e.ToTable(t => t.HasCheckConstraint(
                    "CK_Categories_CategoryType", "[CategoryType] IN ('Age', 'Distance')"));

                e.HasOne(c => c.Event)
                 .WithMany(x => x.Categories)
                 .HasForeignKey(c => c.EventID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Enrolments ----------
            modelBuilder.Entity<Enrolment>(e =>
            {
                e.Property(x => x.EnrolmentDate).HasDefaultValueSql("GETDATE()");

                // A participant cannot enrol in the same category twice
                e.HasIndex(x => new { x.ParticipantID, x.CategoryID }).IsUnique();

                e.HasOne(x => x.Participant)
                 .WithMany(u => u.Enrolments)
                 .HasForeignKey(x => x.ParticipantID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Category)
                 .WithMany(c => c.Enrolments)
                 .HasForeignKey(x => x.CategoryID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Results ----------
            modelBuilder.Entity<Result>(e =>
            {
                e.Property(r => r.DateCaptured).HasDefaultValueSql("GETDATE()");

                // One enrolment has at most one result (1:1), EnrolmentID is UNIQUE
                e.HasOne(r => r.Enrolment)
                 .WithOne(x => x.Result)
                 .HasForeignKey<Result>(r => r.EnrolmentID)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
