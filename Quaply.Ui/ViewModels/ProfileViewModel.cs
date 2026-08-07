using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quaply.Data.Models;
using Quaply.Service.Interfaces;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Parameters;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileViewModel(
    INavigator navigator,
    IProfileService service
) : NavigableViewModel(navigator), INavigationAware
{
    private readonly IProfileService _service = service;

    [ObservableProperty]
    public partial Profile? SelectedProfile { get; set; }

    public ObservableCollection<Profile> Profiles
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public async Task OnNavigatedToAsync(object? parameter)
    {
        await LoadProfilesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateToWorkExperience))]
    private async Task NavigateToWorkExperienceAsync()
    {
        await Navigator.NavigateToAsync<WorkExperienceViewModel>();
    }

    private static bool CanNavigateToWorkExperience()
    {
        return true;
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        IEnumerable<Profile> profiles = await _service.GetProfilesAsync();
        Profiles = new(profiles);
    }

    [RelayCommand]
    private async Task AddProfileAsync()
    {
        await Navigator.NavigateToAsync<ProfileEditorViewModel>(
            new ProfileEditorParameter.Add()
        );
    }

    [RelayCommand]
    private async Task EditProfileAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        await Navigator.NavigateToAsync<ProfileEditorViewModel>(
            new ProfileEditorParameter.Edit(profile.Id)
        );
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(Profile? profile)
    {
        if (profile is null)
        {
            return;
        }

        await _service.DeleteProfileAsync(profile.Id);
        Profiles.Remove(profile);

        if (SelectedProfile == profile)
        {
            SelectedProfile = null;
        }
    }
}
