using Microsoft.Extensions.DependencyInjection;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Utilities;
using Quaply.Ui.ViewModels;
using Quaply.Ui.Views.Pages;

namespace Quaply.Ui;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUi()
        {
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddSingleton<IHostNavigator, HostNavigator>();
            services.AddSingleton<INavigator>(provider =>
                provider.GetRequiredService<IHostNavigator>()
            );

            services.AddTransient<MainViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<WorkExperienceViewModel>();

            services.AddSingleton<ProfilePage>();
            services.AddSingleton<WorkExperiencePage>();
            services.AddSingleton(provider => new MainWindow()
            {
                DataContext = provider.GetRequiredService<MainViewModel>(),
            });
            return services;
        }
    }
}
