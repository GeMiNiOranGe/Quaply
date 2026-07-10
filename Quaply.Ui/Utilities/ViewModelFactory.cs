using Microsoft.Extensions.DependencyInjection;
using Quaply.Ui.Interfaces;
using Quaply.Ui.ViewModels.Base;

namespace Quaply.Ui.Utilities;

public class ViewModelFactory(IServiceProvider serviceProvider)
    : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public ViewModel Create(Type type)
    {
        object service = _serviceProvider.GetRequiredService(type);

        return service as ViewModel
            ?? throw new InvalidOperationException(
                $"ViewModel of type {type.Name} is not registered."
            );
    }
}
