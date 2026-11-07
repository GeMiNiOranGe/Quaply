using CommunityToolkit.Mvvm.ComponentModel;
using Quaply.Data.Models;
using Quaply.Ui.Interfaces;

namespace Quaply.Ui.Models;

/// <summary>
/// Row wrapper for the Profile recovery view: adds selection state and
/// forwards the fields the DataGrid / preview panel bind to.
/// Requires <c>Profile.ResumeProfiles</c> to be loaded (Include) for
/// <see cref="ResumeCount"/> to be accurate.
/// </summary>
public partial class ProfileRecoveryItem(Profile profile)
    : ObservableObject,
        IRecoveryItem
{
    public Profile Profile { get; } = profile;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string DisplayName => Profile.FullName;

    public int Id => Profile.Id;

    public string FullName => Profile.FullName;

    public string? Email => Profile.Email;

    public string? PhoneNumber => Profile.PhoneNumber;

    public string? LinkedInUsername => Profile.LinkedInUsername;

    public string? GitHubUsername => Profile.GitHubUsername;

    public string? PortfolioUrl => Profile.PortfolioUrl;

    public DateOnly? DateOfBirth => Profile.DateOfBirth;

    public DateTime CreatedAt => Profile.CreatedAt;

    public DateTime UpdatedAt => Profile.UpdatedAt;

    public DateTime? DeletedAt => Profile.DeletedAt;

    public int ResumeCount => Profile.ResumeProfiles.Count;

    public string ResumeUsageText =>
        ResumeCount switch
        {
            0 => "Not used in any resume",
            1 => "Used in 1 resume",
            _ => $"Used in {ResumeCount} resumes",
        };

    // Drives the "Contact & links" section: when all of these are empty the
    // preview shows a placeholder instead of an empty block.
    public bool HasContactDetails =>
        PhoneNumber is not null
        || LinkedInUsername is not null
        || GitHubUsername is not null
        || PortfolioUrl is not null
        || DateOfBirth is not null;
}
