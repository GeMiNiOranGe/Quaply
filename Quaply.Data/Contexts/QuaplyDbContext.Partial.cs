using Microsoft.EntityFrameworkCore;
using Quaply.Data.Converters;
using Quaply.Data.Models;

namespace Quaply.Data.Contexts;

public partial class QuaplyDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<SkillCategory>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<ProjectType>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<PersonalSummary>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity
                .Property(e => e.CreatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.UpdatedAt)
                .HasConversion<UtcDateTimeConverter>();

            entity
                .Property(e => e.DeletedAt)
                .HasConversion<NullableUtcDateTimeConverter>();
        });
    }
}
