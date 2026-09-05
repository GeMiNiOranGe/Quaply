using Microsoft.Extensions.DependencyInjection;
using Quaply.Service.Interfaces;

namespace Quaply.Service;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddService()
        {
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IWorkExperienceService, WorkExperienceService>();

            return services;
        }
    }
}
