using Microsoft.Extensions.DependencyInjection;

namespace Quaply.Ui;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUi()
        {
            services.AddSingleton<MainWindow>();
            return services;
        }
    }
}
