using CommunityToolkit.Mvvm.Input;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public partial class WorkExperienceViewModel(INavigator navigator)
    : NavigableViewModel(navigator)
{
    [RelayCommand(CanExecute = nameof(CanNavigateToProfile))]
    private void NavigateToProfile()
    {
        Navigator.NavigateTo<ProfileViewModel>();
    }

    private static bool CanNavigateToProfile()
    {
        return true;
    }
}
