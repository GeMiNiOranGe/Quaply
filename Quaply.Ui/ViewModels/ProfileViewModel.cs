using CommunityToolkit.Mvvm.Input;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class ProfileViewModel(INavigator navigator)
    : NavigableViewModel(navigator)
{
    [RelayCommand(CanExecute = nameof(CanNavigateToWorkExperience))]
    private void NavigateToWorkExperience()
    {
        Navigator.NavigateTo<WorkExperienceViewModel>();
    }

    private static bool CanNavigateToWorkExperience()
    {
        return true;
    }
}
