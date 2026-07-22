using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;

namespace Quaply.Data;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddData()
        {
            string databasePath = Path.Combine(
                AppContext.BaseDirectory,
                "Database",
                "Quaply.db"
            );

            services.AddDbContext<QuaplyDbContext>(options =>
                options.UseSqlite($"Data Source={databasePath}")
            );

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
