using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Parameters;
using Quaply.Ui.Validations;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileEditorViewModel(
    INavigator navigator,
    IProfileService service
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;

    // Only Edit needs to remember which row gets updated on Save.
    // Add and (future) Duplicate always create a new row.
    private int? _editingProfileId;

    [ObservableProperty]
    public partial bool IsEditMode { get; private set; }

    [ObservableProperty]
    public partial string PageTitle { get; private set; } = "Add Profile";

    [ObservableProperty]
    public partial bool IsSaving { get; private set; }

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [MaxLength(255, ErrorMessage = "Full name must be at most 255 characters.")]
    [ObservableProperty]
    [Required(ErrorMessage = "Full name is required.")]
    public partial string FullName { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    [OptionalEmailAddress(ErrorMessage = "Email is not valid.")]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    [OptionalPhone(ErrorMessage = "Phone number is not valid.")]
    public partial string PhoneNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LinkedInUsername { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GitHubUsername { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    [OptionalUrl(ErrorMessage = "Portfolio URL is not valid.")]
    public partial string PortfolioUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateOnly? DateOfBirth { get; set; }

    public async Task OnNavigatedToAsync(object? parameter)
    {
        switch (parameter)
        {
            case ProfileEditorParameter.Edit edit:
                await EnterEditModeAsync(edit.ProfileId);
                break;
            // case ProfileEditorParameter.Duplicate duplicate:
            //     await EnterDuplicateModeAsync(duplicate.SourceProfileId);
            //     break;
            case ProfileEditorParameter.Add:
            default:
                ResetToAddMode();
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            return;
        }

        IsSaving = true;

        try
        {
            if (IsEditMode && _editingProfileId is int id)
            {
                Profile? existing = await _service.GetProfileByIdAsync(id);
                if (existing is null)
                {
                    return;
                }

                ApplyFormTo(existing);
                await _service.UpdateProfileAsync(existing);
            }
            else
            {
                // Covers both Add and (future) Duplicate: always a new row.
                Profile profile = new();
                ApplyFormTo(profile);
                await _service.CreateProfileAsync(profile);
            }

            await Navigator.NavigateToAsync<ProfileViewModel>();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(FullName) && !HasErrors && !IsSaving;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Navigator.NavigateToAsync<ProfileViewModel>();
    }

    private async Task EnterEditModeAsync(int profileId)
    {
        Profile? profile = await _service.GetProfileByIdAsync(profileId);
        if (profile is null)
        {
            ResetToAddMode();
            return;
        }

        _editingProfileId = profile.Id;
        IsEditMode = true;
        PageTitle = "Edit Profile";

        FillFormFrom(profile);
        ClearErrors();
    }

    // Reserved for future use - uncomment alongside ProfileEditorParameter.Duplicate.
    // private async Task EnterDuplicateModeAsync(int sourceProfileId)
    // {
    //     Profile? source = await _service.GetProfileByIdAsync(sourceProfileId);
    //     if (source is null)
    //     {
    //         ResetToAddMode();
    //         return;
    //     }
    //
    //     _editingProfileId = null; // Save must create a new row, not update the source.
    //     IsEditMode = false;
    //     PageTitle = "Duplicate Profile";
    //
    //     FillFormFrom(source);
    //     ClearErrors();
    // }

    private void ResetToAddMode()
    {
        _editingProfileId = null;
        IsEditMode = false;
        PageTitle = "Add Profile";

        FullName = string.Empty;
        Title = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        LinkedInUsername = string.Empty;
        GitHubUsername = string.Empty;
        PortfolioUrl = string.Empty;
        DateOfBirth = null;

        ClearErrors();
    }

    // Shared by Edit and (future) Duplicate - both pre-fill the form the same way.
    private void FillFormFrom(Profile profile)
    {
        FullName = profile.FullName;
        Title = profile.Title ?? string.Empty;
        Email = profile.Email ?? string.Empty;
        PhoneNumber = profile.PhoneNumber ?? string.Empty;
        LinkedInUsername = profile.LinkedInUsername ?? string.Empty;
        GitHubUsername = profile.GitHubUsername ?? string.Empty;
        PortfolioUrl = profile.PortfolioUrl ?? string.Empty;
        DateOfBirth = profile.DateOfBirth;
    }

    private void ApplyFormTo(Profile profile)
    {
        profile.FullName = FullName.Trim();
        profile.Title = string.IsNullOrWhiteSpace(Title) ? null : Title.Trim();
        profile.Email = Email.Trim();
        profile.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber)
            ? null
            : PhoneNumber.Trim();
        profile.LinkedInUsername = string.IsNullOrWhiteSpace(LinkedInUsername)
            ? null
            : LinkedInUsername.Trim();
        profile.GitHubUsername = string.IsNullOrWhiteSpace(GitHubUsername)
            ? null
            : GitHubUsername.Trim();
        profile.PortfolioUrl = string.IsNullOrWhiteSpace(PortfolioUrl)
            ? null
            : PortfolioUrl.Trim();
        profile.DateOfBirth = DateOfBirth;
    }
}
