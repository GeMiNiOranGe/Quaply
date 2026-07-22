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
    private void AddProfile()
    {
        // TODO: Navigator.NavigateTo<AddProfileViewModel>();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProfile))]
    private void EditProfile()
    {
        // TODO: Navigator.NavigateTo<EditProfileViewModel>(SelectedProfile!.Id);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProfile))]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        await _service.DeleteProfileAsync(SelectedProfile.Id);
        Profiles.Remove(SelectedProfile);
        SelectedProfile = null;
    }

    private bool HasSelectedProfile()
    {
        return SelectedProfile is not null;
    }

    partial void OnSelectedProfileChanged(Profile? value)
    {
        EditProfileCommand.NotifyCanExecuteChanged();
        DeleteProfileCommand.NotifyCanExecuteChanged();
    }
}
