using Microsoft.Extensions.DependencyInjection;
using Quaply.Ui.Interfaces;
using Quaply.Ui.Utilities;
using Quaply.Ui.ViewModels;
using Quaply.Ui.Views.Pages;
using Wpf.Ui;

namespace Quaply.Ui;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUi()
        {
            services.AddSingleton<
                IContentDialogService,
                ContentDialogService
            >();

            services.AddSingleton<IDialogPresenter, DialogPresenter>();
            services.AddSingleton<IHostNavigator, HostNavigator>();
            services.AddSingleton<INavigator>(provider =>
                provider.GetRequiredService<IHostNavigator>()
            );
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<ProfileEditorViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<WorkExperienceViewModel>();

            services.AddSingleton<ProfileEditorPage>();
            services.AddSingleton<ProfilePage>();
            services.AddSingleton<WorkExperiencePage>();
            services.AddSingleton(provider => new MainWindow(
                provider.GetRequiredService<IContentDialogService>()
            )
            {
                DataContext = provider.GetRequiredService<MainViewModel>(),
            });
            return services;
        }
    }
}
