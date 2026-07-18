using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Interfaces;

public interface INavigator
{
    Task NavigateToAsync<TViewModel>(object? parameter = null)
        where TViewModel : ViewModel;
}
