using Microsoft.Extensions.DependencyInjection;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Utilities;

namespace Quaply.Ui;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUi()
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddSingleton<IHostNavigator, HostNavigator>();
            services.AddSingleton<INavigator>(provider =>
                provider.GetRequiredService<IHostNavigator>()
            );
            return services;
        }
    }
}
