using DbUp.Postgresql;
using Npgsql;

namespace Storczyk.Database.Services;

public static class PostgresUpgraderExtensions
{
    public static IHostApplicationBuilder AddPostgresUpgrader(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<PostgresqlConnectionManager>(
            services => new PostgresqlConnectionManager(services.GetRequiredService<NpgsqlDataSource>())
        );
        builder.Services.AddTransient<PostgresUpgrader>(
            services => new PostgresUpgrader(
                services.GetRequiredService<PostgresqlConnectionManager>(),
                services.GetRequiredService<ILogger<PostgresUpgrader>>())
        );
        
        return builder;
    }
}