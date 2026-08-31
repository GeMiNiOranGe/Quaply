using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileTrashViewModel(
    INavigator navigator,
    IProfileService service,
    IDialogPresenter dialogPresenter
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;
    private readonly IDialogPresenter _dialogPresenter = dialogPresenter;

    [ObservableProperty]
    public partial Profile? SelectedProfile { get; set; }

    public ObservableCollection<Profile> DeletedProfiles
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public async Task OnNavigatedToAsync()
    {
        await LoadDeletedProfilesAsync();
    }

    [RelayCommand]
    private async Task LoadDeletedProfilesAsync()
    {
        IEnumerable<Profile> profiles =
            await _service.GetDeletedProfilesAsync();
        DeletedProfiles = new(profiles);
    }

    [RelayCommand]
    private async Task RestoreProfileAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        await _service.RestoreProfileAsync(profile.Id);
        DeletedProfiles.Remove(profile);

        if (SelectedProfile == profile)
        {
            SelectedProfile = null;
        }
    }

    [RelayCommand]
    private async Task PurgeProfileAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        bool confirmed = await _dialogPresenter.ShowConfirmationAsync(
            title: "Confirm permanent deletion",
            message: $"Permanently delete the profile '{profile.FullName}'? This action cannot be undone.",
            primaryButtonText: "Delete permanently",
            closeButtonText: "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        await _service.PurgeProfileAsync(profile.Id);
        DeletedProfiles.Remove(profile);

        if (SelectedProfile == profile)
        {
            SelectedProfile = null;
        }
    }

    [RelayCommand]
    private async Task BackToProfilesAsync()
    {
        await Navigator.NavigateToAsync<ProfileViewModel>();
    }
}
