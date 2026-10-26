using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quaply.Data.Contexts;
using Quaply.Data.Interfaces;
#if DEBUG
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Quaply.Data.Diagnostics;
#endif

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

#if DEBUG
            services.AddSingleton<SqlConsoleInterceptor>();
#endif

            services.AddDbContext<QuaplyDbContext>(
                (provider, options) =>
                {
                    options.UseSqlite($"Data Source={databasePath}");

#if DEBUG
                    options.AddInterceptors(
                        provider.GetRequiredService<SqlConsoleInterceptor>()
                    );

                    // lower EF's default log level to Trace
                    // to avoid duplicate logging with interceptor.
                    options.ConfigureWarnings(w =>
                        w.Log(
                            (RelationalEventId.CommandExecuted, LogLevel.Trace),
                            (RelationalEventId.CommandError, LogLevel.Trace)
                        )
                    );
#endif
                }
            );

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
