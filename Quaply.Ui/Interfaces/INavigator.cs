using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Interfaces;

public interface INavigator
{
    Task NavigateToAsync<TViewModel>()
        where TViewModel : ViewModel;
    Task NavigateToAsync<TViewModel, TParameter>(TParameter parameter)
        where TViewModel : ViewModel, INavigationAware<TParameter>;
}
