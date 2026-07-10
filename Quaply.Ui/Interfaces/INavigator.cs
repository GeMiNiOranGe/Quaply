using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Interfaces;

public interface INavigator
{
    void NavigateTo<TViewModel>()
        where TViewModel : ViewModel;
}
