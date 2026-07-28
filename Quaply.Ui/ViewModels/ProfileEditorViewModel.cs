using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Validations;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileEditorViewModel(
    INavigator navigator,
    IProfileService service
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;
    private int _profileId;

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
        if (parameter is int id && id > 0)
        {
            await LoadProfileAsync(id);
            return;
        }

        ResetToAddMode();
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
            if (IsEditMode)
            {
                Profile? existing = await _service.GetProfileByIdAsync(
                    _profileId
                );
                if (existing is null)
                {
                    return;
                }

                ApplyFormTo(existing);
                await _service.UpdateProfileAsync(existing);
            }
            else
            {
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

    private async Task LoadProfileAsync(int id)
    {
        Profile? profile = await _service.GetProfileByIdAsync(id);
        if (profile is null)
        {
            ResetToAddMode();
            return;
        }

        _profileId = profile.Id;
        IsEditMode = true;
        PageTitle = "Edit Profile";

        FullName = profile.FullName;
        Title = profile.Title ?? string.Empty;
        Email = profile.Email ?? string.Empty;
        PhoneNumber = profile.PhoneNumber ?? string.Empty;
        LinkedInUsername = profile.LinkedInUsername ?? string.Empty;
        GitHubUsername = profile.GitHubUsername ?? string.Empty;
        PortfolioUrl = profile.PortfolioUrl ?? string.Empty;
        DateOfBirth = profile.DateOfBirth;

        ClearErrors();
    }

    private void ResetToAddMode()
    {
        _profileId = 0;
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
