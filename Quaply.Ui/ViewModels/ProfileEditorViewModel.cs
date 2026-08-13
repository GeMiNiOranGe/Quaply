using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Validations;
using Quaply.Ui.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Quaply.Ui.ViewModels;

public partial class ProfileEditorViewModel(
    INavigator navigator,
    IProfileService service,
    IDialogPresenter dialogPresenter
) : NavigableViewModel(navigator), INavigationAware
{
    private sealed record ProfileFormSnapshot(
        string FullName,
        string Title,
        string Email,
        string PhoneNumber,
        string LinkedInUsername,
        string GitHubUsername,
        string PortfolioUrl,
        DateOnly? DateOfBirth
    );

    private readonly IProfileService _service = service;
    private readonly IDialogPresenter _dialogPresenter = dialogPresenter;

    // Only Edit needs to remember which row gets updated on Save.
    // Add and (future) Duplicate always create a new row.
    private int? _editingProfileId;

    // A baseline for comparing the dirty state. It is updated whenever the mode
    // changes or after a reset is completed.
    private ProfileFormSnapshot _baseline = EmptySnapshot();

    [ObservableProperty]
    public partial string PageTitle { get; private set; }

    [ObservableProperty]
    public partial string ResetButtonText { get; private set; }

    [ObservableProperty]
    public partial SymbolRegular ResetButtonIcon { get; private set; }

    [ObservableProperty]
    public partial string SaveButtonText { get; private set; }

    [ObservableProperty]
    public partial bool IsSaving { get; private set; }

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [MaxLength(255, ErrorMessage = "Full name must be at most 255 characters.")]
    [ObservableProperty]
    [Required(ErrorMessage = "Full name is required.")]
    public partial string FullName { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    [OptionalEmailAddress(ErrorMessage = "Email is not valid.")]
    public partial string Email { get; set; } = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    [OptionalPhone(ErrorMessage = "Phone number is not valid.")]
    public partial string PhoneNumber { get; set; } = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [ObservableProperty]
    public partial string LinkedInUsername { get; set; } = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [ObservableProperty]
    public partial string GitHubUsername { get; set; } = string.Empty;

    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty]
    [OptionalUrl(ErrorMessage = "Portfolio URL is not valid.")]
    public partial string PortfolioUrl { get; set; } = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [ObservableProperty]
    public partial DateOnly? DateOfBirth { get; set; }

    public async Task OnNavigatedToAsync(object? parameter)
    {
        switch (parameter)
        {
            case ProfileEditorParameter.Edit edit:
                await EnterEditModeAsync(edit);
                break;
            case ProfileEditorParameter.Duplicate duplicate:
                await EnterDuplicateModeAsync(duplicate);
                break;
            case ProfileEditorParameter.Add add:
                EnterAddMode(add);
                break;
            default:
                EnterAddMode(new());
                break;
        }
    }

    /// <summary>
    /// Current navigation parameter driving this editor's mode.
    /// Assigning this also derives <see cref="PageTitle"/> and
    /// <see cref="SaveButtonText"/> — do not set those two directly.
    /// </summary>
    private ProfileEditorParameter EditorParameter
    {
        get;
        set
        {
            field = value;
            (PageTitle, ResetButtonText, ResetButtonIcon, SaveButtonText) =
                value switch
                {
                    ProfileEditorParameter.Edit => (
                        "Edit Profile",
                        "Reset Changes",
                        SymbolRegular.ArrowReset24,
                        "Save Changes"
                    ),
                    ProfileEditorParameter.Duplicate => (
                        "Duplicate Profile",
                        "Reset Changes",
                        SymbolRegular.ArrowReset24,
                        "Create Duplicate"
                    ),
                    ProfileEditorParameter.Add or _ => (
                        "Create Profile",
                        "Clear",
                        SymbolRegular.Eraser24,
                        "Create Profile"
                    ),
                };
        }
    } = new ProfileEditorParameter.Add();

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
            if (_editingProfileId is int id)
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
                // Covers both Add and Duplicate: always a new row.
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

    [RelayCommand(CanExecute = nameof(CanReset))]
    private async Task ResetAsync()
    {
        bool confirmed = await _dialogPresenter.ShowDangerConfirmationAsync(
            title: "Discard changes?",
            message: "Your unsaved changes will be lost. This action cannot be undone.",
            primaryButtonText: "Discard",
            closeButtonText: "Keep editing"
        );

        if (!confirmed)
        {
            return;
        }

        switch (EditorParameter)
        {
            case ProfileEditorParameter.Edit edit:
                await ReloadFormFromAsync(edit.Id);
                break;
            case ProfileEditorParameter.Duplicate duplicate:
                await ReloadFormFromAsync(duplicate.SourceId);
                break;
            case ProfileEditorParameter.Add:
            default:
                ClearForm();
                ClearErrors();
                CaptureBaseline();
                break;
        }
    }

    // Dirty check: Only allow a reset if the form differs from
    // the current baseline.
    private bool CanReset()
    {
        return !IsSaving && CurrentSnapshot() != _baseline;
    }

    // Shared by Edit and Duplicate: both "reload from an existing profile ID."
    private async Task ReloadFormFromAsync(int profileId)
    {
        Profile? profile = await _service.GetProfileByIdAsync(profileId);
        if (profile is not null)
        {
            FillFormFrom(profile);
        }

        ClearErrors();
        CaptureBaseline();
    }

    private void EnterAddMode(ProfileEditorParameter.Add add)
    {
        _editingProfileId = null;

        EditorParameter = add;
        ClearForm();
        ClearErrors();
        CaptureBaseline();
    }

    private async Task EnterEditModeAsync(ProfileEditorParameter.Edit edit)
    {
        Profile? profile = await _service.GetProfileByIdAsync(edit.Id);
        if (profile is null)
        {
            EnterAddMode(new ProfileEditorParameter.Add());
            return;
        }

        // Update the source.
        _editingProfileId = edit.Id;

        EditorParameter = edit;
        FillFormFrom(profile);
        ClearErrors();
        CaptureBaseline();
    }

    private async Task EnterDuplicateModeAsync(
        ProfileEditorParameter.Duplicate duplicate
    )
    {
        Profile? source = await _service.GetProfileByIdAsync(
            duplicate.SourceId
        );
        if (source is null)
        {
            EnterAddMode(new ProfileEditorParameter.Add());
            return;
        }

        // Save must create a new row, not update the source.
        _editingProfileId = null;

        EditorParameter = duplicate;
        FillFormFrom(source);
        ClearErrors();
        CaptureBaseline();
    }

    private void ClearForm()
    {
        FullName = string.Empty;
        Title = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        LinkedInUsername = string.Empty;
        GitHubUsername = string.Empty;
        PortfolioUrl = string.Empty;
        DateOfBirth = null;
    }

    // Shared by Edit and Duplicate - both pre-fill the form the same way.
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

    // Capture the current form state to serve as a baseline for dirty checking.
    private void CaptureBaseline()
    {
        _baseline = CurrentSnapshot();
        ResetCommand.NotifyCanExecuteChanged();
    }

    private ProfileFormSnapshot CurrentSnapshot()
    {
        return new(
            FullName,
            Title,
            Email,
            PhoneNumber,
            LinkedInUsername,
            GitHubUsername,
            PortfolioUrl,
            DateOfBirth
        );
    }

    private static ProfileFormSnapshot EmptySnapshot()
    {
        return new(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            null
        );
    }
}
