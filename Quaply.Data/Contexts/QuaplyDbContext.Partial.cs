using Microsoft.EntityFrameworkCore;
using Quaply.Data.Converters;
using Quaply.Data.Models;

namespace Quaply.Data.Contexts;

public partial class QuaplyDbContext
{
    // See Docs/DATABASE.md#timestamps
    private static readonly HashSet<string> LifecycleOnly =
    [
        "UpdatedAt",
        "ExportedAt",
        "DeletedAt",
    ];

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyUpdatedAt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        ApplyUpdatedAt();
        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken
        );
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder
    )
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder
            .Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<SkillCategory>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<ProjectType>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<PersonalSummary>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.HasQueryFilter(e => e.Project.DeletedAt == null);
        });

        modelBuilder.Entity<ResumeCertification>(entity =>
        {
            entity.HasQueryFilter(e => e.Certification.DeletedAt == null);
        });

        modelBuilder.Entity<ResumeEducation>(entity =>
        {
            entity.HasQueryFilter(e => e.Education.DeletedAt == null);
        });

        modelBuilder.Entity<ResumeLanguage>(entity =>
        {
            entity.HasQueryFilter(e => e.Language.DeletedAt == null);
        });

        modelBuilder.Entity<ResumePersonalSummary>(entity =>
        {
            entity.HasQueryFilter(e => e.PersonalSummary.DeletedAt == null);
        });

        modelBuilder.Entity<ResumeProfile>(entity =>
        {
            entity.HasQueryFilter(e => e.Profile.DeletedAt == null);
        });

        modelBuilder.Entity<ResumeWorkExperience>(entity =>
        {
            entity.HasQueryFilter(e => e.WorkExperience.DeletedAt == null);
        });
    }

    private void ApplyUpdatedAt()
    {
        DateTime now = DateTime.UtcNow;

        foreach (
            var entry in ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Modified)
        )
        {
            if (entry.Metadata.FindProperty("UpdatedAt") is null)
            {
                // junction tables
                continue;
            }

            bool contentChanged = entry.Properties.Any(p =>
                p.IsModified
                && !LifecycleOnly.Contains(p.Metadata.Name)
                && !Equals(p.OriginalValue, p.CurrentValue)
            );

            if (contentChanged)
            {
                entry.Property("UpdatedAt").CurrentValue = now;
            }
        }
    }
}
