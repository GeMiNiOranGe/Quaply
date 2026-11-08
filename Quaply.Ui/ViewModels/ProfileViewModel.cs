using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileViewModel(
    INavigator navigator,
    IProfileService service
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;

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
        await Navigator.NavigateToAsync<WorkExperienceViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToRecoveryAsync()
    {
        await Navigator.NavigateToAsync<ProfileRecoveryViewModel>();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForAdd());
    }

    [RelayCommand]
    private async Task EditAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForEdit(profile.Id));
    }

    [RelayCommand]
    private async Task DuplicateAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<
            ProfileEditorViewModel,
            ProfileEditorParameter
        >(ProfileEditorParameter.ForDuplicate(profile.Id));
    }

    [RelayCommand]
    private async Task DeleteAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        await _service.DeleteProfileAsync(profile.Id);
        Items.Remove(profile);

        if (SelectedItem == profile)
        {
            SelectedItem = null;
        }
    }

    private async Task LoadAsync()
    {
        IEnumerable<Profile> profiles = await _service.GetProfilesAsync();
        Items = new(profiles);
    }
}
