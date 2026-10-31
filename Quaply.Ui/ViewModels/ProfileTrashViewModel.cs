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
    public partial Profile? SelectedItem { get; set; }

    public ObservableCollection<Profile> Items
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
        await LoadAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Navigator.NavigateToAsync<ProfileViewModel>();
    }

    [RelayCommand]
    private async Task RestoreAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        await _service.RestoreProfileAsync(profile.Id);
        Items.Remove(profile);

        if (SelectedItem == profile)
        {
            SelectedItem = null;
        }
    }

    [RelayCommand]
    private async Task PurgeAsync(Profile? profile)
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
        Items.Remove(profile);

        if (SelectedItem == profile)
        {
            SelectedItem = null;
        }
    }

    private async Task LoadAsync()
    {
        IEnumerable<Profile> deletedProfiles =
            await _service.GetDeletedProfilesAsync();
        Items = new(deletedProfiles);
    }
}
