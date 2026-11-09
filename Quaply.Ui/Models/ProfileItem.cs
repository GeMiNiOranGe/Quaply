using CommunityToolkit.Mvvm.ComponentModel;
using Quaply.Data.Models;

namespace Quaply.Ui.Models;

/// <summary>
/// Row model for the Profile list. Wraps a <see cref="Profile"/> and adds
/// UI-only state (selection) plus display helpers, so the XAML stays free of
/// formatting logic.
/// </summary>
public partial class ProfileItem(Profile profile) : ObservableObject
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
}
