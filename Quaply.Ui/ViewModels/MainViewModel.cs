using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.ViewModels;

public class MainViewModel : NavigableViewModel, IDisposable
{
    private readonly IHostNavigator _hostNavigator;
    private readonly AsyncRelayCommand _navigateToProfileCommand;
    private readonly AsyncRelayCommand _navigateToWorkExperienceCommand;

    public MainViewModel(IHostNavigator hostNavigator)
        : base(hostNavigator)
    {
        _hostNavigator = hostNavigator;

        if (_hostNavigator is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged += OnNavigatorPropertyChanged;
        }

        _navigateToProfileCommand = new AsyncRelayCommand(
            NavigateToProfileAsync,
            CanNavigateToProfile
        );
        _navigateToWorkExperienceCommand = new AsyncRelayCommand(
            NavigateToWorkExperienceAsync,
            CanNavigateToWorkExperience
        );
    }

    public ViewModel? CurrentViewModel => _hostNavigator.Current;

    public bool IsProfileSelected => CurrentViewModel is ProfileIndexViewModel;

    public bool IsWorkExperienceSelected =>
        CurrentViewModel is WorkExperienceIndexViewModel;

    public ICommand NavigateToProfileCommand => _navigateToProfileCommand;

    public ICommand NavigateToWorkExperienceCommand =>
        _navigateToWorkExperienceCommand;

    public void Dispose()
    {
        if (_hostNavigator is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged -= OnNavigatorPropertyChanged;
        }

        GC.SuppressFinalize(this);
    }

    private async Task NavigateToProfileAsync()
    {
        await _hostNavigator.NavigateToAsync<ProfileIndexViewModel>();
    }

    private bool CanNavigateToProfile()
    {
        return _hostNavigator.Current is not ProfileIndexViewModel;
    }

    private async Task NavigateToWorkExperienceAsync()
    {
        await _hostNavigator.NavigateToAsync<WorkExperienceIndexViewModel>();
    }

    private bool CanNavigateToWorkExperience()
    {
        return _hostNavigator.Current is not WorkExperienceIndexViewModel;
    }

    private void OnNavigatorPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e
    )
    {
        if (e.PropertyName == nameof(IHostNavigator.Current))
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(IsProfileSelected));
            OnPropertyChanged(nameof(IsWorkExperienceSelected));

            // Notify WPF that CanExecute() needs to be re-evaluated immediately.
            _navigateToProfileCommand.NotifyCanExecuteChanged();
            _navigateToWorkExperienceCommand.NotifyCanExecuteChanged();
        }
    }
}
