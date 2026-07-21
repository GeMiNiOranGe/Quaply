namespace Quaply.Ui.Interfaces;

public interface INavigationAware
{
    Task OnNavigatedFromAsync() => Task.CompletedTask;
    Task OnNavigatedToAsync(object? parameter) => Task.CompletedTask;
}
