using Quaply.Ui.Interfaces;

namespace Quaply.Ui.ViewModels.Base;

public abstract class NavigableViewModel(INavigator navigator) : ViewModel
{
    private readonly INavigator _navigator = navigator;

    protected INavigator Navigator => _navigator;
}
