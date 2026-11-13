using CommunityToolkit.Mvvm.ComponentModel;
using Quaply.Data.Models;
using Quaply.Ui.Interfaces;

namespace Quaply.Ui.Models;

/// <summary>
/// Row model for the Profile list. Wraps a <see cref="Profile"/> and adds
/// UI-only state (selection) plus display helpers, so the XAML stays free of
/// formatting logic.
/// </summary>
public partial class ProfileItem(Profile profile) : ObservableObject, IIndexItem
{
    public Profile Profile { get; } = profile;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

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

    public int ResumeCount => Profile.ResumeProfiles.Count;

    public string ResumeUsageText =>
        ResumeCount switch
        {
            0 => "Not used in any resume",
            1 => "Used in 1 resume",
            _ => $"Used in {ResumeCount} resumes",
        };

    // True when there is anything to show in the "Contact & links" section
    // (email is shown separately, in the identity block).
    public bool HasContactDetails =>
        !string.IsNullOrWhiteSpace(PhoneNumber)
        || !string.IsNullOrWhiteSpace(LinkedInUsername)
        || !string.IsNullOrWhiteSpace(GitHubUsername)
        || !string.IsNullOrWhiteSpace(PortfolioUrl)
        || DateOfBirth is not null;

    // A CV header without an email or a phone number is not very useful,
    // so flag those profiles instead of letting the user find out at export.
    public IReadOnlyList<string> MissingFields { get; } =
        BuildMissingFields(profile);

    public bool IsComplete => MissingFields.Count == 0;

    public string? MissingFieldsText =>
        IsComplete ? null : $"Missing: {string.Join(", ", MissingFields)}";

    private static List<string> BuildMissingFields(Profile profile)
    {
        List<string> missing = [];

        if (string.IsNullOrWhiteSpace(profile.Email))
        {
            missing.Add("email");
        }

        if (string.IsNullOrWhiteSpace(profile.PhoneNumber))
        {
            missing.Add("phone number");
        }

        return missing;
    }
}
