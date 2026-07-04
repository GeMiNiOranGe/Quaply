using Microsoft.Extensions.DependencyInjection;

namespace Quaply.Service;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddService()
        {
            return services;
        }
    }
}
