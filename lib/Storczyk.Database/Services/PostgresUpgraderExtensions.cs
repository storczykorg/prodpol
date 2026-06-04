using DbUp.Postgresql;
using Npgsql;

namespace Storczyk.Database.Services;

public static class PostgresUpgraderExtensions
{
    public static IServiceCollection AddPostgresUpgrader(this IServiceCollection builder)
    {
        builder.AddTransient<PostgresqlConnectionManager>(services =>
            new PostgresqlConnectionManager(services.GetRequiredService<NpgsqlDataSource>())
        );
        builder.AddTransient<PostgresUpgrader>(services => new PostgresUpgrader(
            services.GetRequiredService<PostgresqlConnectionManager>(),
            services.GetRequiredService<ILogger<PostgresUpgrader>>())
        );
        builder.AddTransient<PostgresSeedUpgrader>(services => new PostgresSeedUpgrader(
            services.GetRequiredService<PostgresqlConnectionManager>(),
            services.GetRequiredService<ILogger<PostgresUpgrader>>())
        );

        return builder;
    }
}