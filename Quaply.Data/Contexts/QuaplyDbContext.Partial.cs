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
}
