namespace Quaply.Ui.Interfaces;

public interface INavigationAware<TParameter> : INavigationAware
{
    Task OnNavigatedToAsync(TParameter parameter) => Task.CompletedTask;
}
