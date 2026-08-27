using Microsoft.EntityFrameworkCore;
using Quaply.Data.Extensions;
using Quaply.Data.Models;

namespace Quaply.Data.Contexts;

public partial class QuaplyDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<SkillCategory>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<ProjectType>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<PersonalSummary>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.Property(e => e.CreatedAt).HasUtcConversion();
            entity.Property(e => e.UpdatedAt).HasUtcConversion();
            entity.Property(e => e.DeletedAt).HasUtcConversion();
        });
    }
}
