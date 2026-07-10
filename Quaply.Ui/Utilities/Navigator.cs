using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Utilities;

public class HostNavigator(IViewModelFactory viewModelFactory)
    : ViewModel,
        IHostNavigator
{
    private readonly IViewModelFactory _viewModelFactory = viewModelFactory;
    private ViewModel? _currentViewModel;

    public ViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public void NavigateTo<TViewModel>()
        where TViewModel : ViewModel
    {
        ViewModel viewModel = _viewModelFactory.Create(typeof(TViewModel));
        CurrentViewModel = viewModel;
    }
}
