using Storczyk.Prodpol.Core.Data;
using Storczyk.Prodpol.Core.Services;

namespace Storczyk.Database.Services;

public static class PostgresRepositoriesExtensions
{
    public static IServiceCollection AddPostgresRepositories(this IServiceCollection services)
    {
        services.AddTransient<IEmployeesRepository, PgEmployeeRepository>();
        services.AddTransient<IEmployeeRoleRepository, PgEmployeeRoleRepository>();

        return services;
    }
    public static IServiceCollection AddNullRepositories(this IServiceCollection services)
    {
        services.AddTransient<IEmployeesRepository, NullEmployeeRepository>();
        services.AddTransient<IEmployeeRoleRepository, NullEmployeeRoleRepository>();

        return services;
    }
}