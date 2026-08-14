using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Utilities;

public class HostNavigator(IViewModelFactory viewModelFactory)
    : ViewModel,
        IHostNavigator
{
    private readonly IViewModelFactory _viewModelFactory = viewModelFactory;
    private ViewModel? _current;

    public ViewModel? Current
    {
        get => _current;
        private set
        {
            _current = value;
            OnPropertyChanged();
        }
    }

    public async Task NavigateToAsync<TViewModel>()
        where TViewModel : ViewModel
    {
        if (Current is INavigationAware currentAware)
        {
            await currentAware.OnNavigatedFromAsync();
        }

        ViewModel viewModel = _viewModelFactory.Create(typeof(TViewModel));
        Current = viewModel;

        if (viewModel is INavigationAware newAware)
        {
            await newAware.OnNavigatedToAsync();
        }
    }

    public async Task NavigateToAsync<TViewModel, TParameter>(
        TParameter parameter
    )
        where TViewModel : ViewModel, INavigationAware<TParameter>
    {
        if (Current is INavigationAware currentAware)
        {
            await currentAware.OnNavigatedFromAsync();
        }

        ViewModel viewModel = _viewModelFactory.Create(typeof(TViewModel));
        Current = viewModel;

        if (viewModel is INavigationAware<TParameter> newAware)
        {
            await newAware.OnNavigatedToAsync(parameter);
        }
    }
}
