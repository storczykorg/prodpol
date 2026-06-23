using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Sorters;
using DbUp.Postgresql;
using DbUp.Support;

namespace Storczyk.Database.Services;

internal class QuietUpgradeLog(ILogger logger) : IUpgradeLog
{
    public void LogInformation(string format, params object[] args) =>
        logger.LogDebug(format, args);

    public void LogTrace(string format, params object[] args) =>
        logger.LogTrace(format, args);

    public void LogDebug(string format, params object[] args) =>
        logger.LogDebug(format, args);

    public void LogWarning(string format, params object[] args) =>
        logger.LogWarning(format, args);

    public void LogError(string format, params object[] args) =>
        logger.LogError(format, args);

    public void LogError(Exception exception, string format, params object[] args) =>
        logger.LogError(exception, format, args);
}

public class PostgresUpgrader(
    PostgresqlConnectionManager manager,
    ILogger<PostgresUpgrader> logger
)
{
    public virtual UpgradeEngine Build()
    {
        var quietLog = new QuietUpgradeLog(logger);
        var builder = PostgresqlExtensions.PostgresqlDatabase(manager, "prodpol");
        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".schemas."),
            new SqlScriptOptions { RunGroupOrder = 1, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".types."),
            new SqlScriptOptions { RunGroupOrder = 3 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".utils."),
            new SqlScriptOptions { RunGroupOrder = 5 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".dictionaries."),
            new SqlScriptOptions { RunGroupOrder = 10 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".tables."),
            new SqlScriptOptions { RunGroupOrder = 13 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".checks."),
            new SqlScriptOptions { RunGroupOrder = 15 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".foreign_keys."),
            new SqlScriptOptions { RunGroupOrder = 20 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".indices."),
            new SqlScriptOptions { RunGroupOrder = 25 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".views."),
            new SqlScriptOptions { RunGroupOrder = 30 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".procedures."),
            new SqlScriptOptions { RunGroupOrder = 35 });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".reports."),
            new SqlScriptOptions { RunGroupOrder = 40 });
        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".triggers."),
            new SqlScriptOptions { RunGroupOrder = 45 });

        builder.WithScriptSorter(new DefaultScriptSorter());

        builder.LogTo(quietLog);
        builder.WithTransaction();

        builder.JournalTo(new ProdpolTableJournal(
            () => manager,
            () => quietLog,
            "prodpol_meta",
            "migrations"
        ));

        return builder.Build();
    }
}

public class PostgresSeedUpgrader(
    PostgresqlConnectionManager manager,
    ILogger<PostgresSeedUpgrader> logger
)
{
    public virtual UpgradeEngine Build()
    {
        var quietLog = new QuietUpgradeLog(logger);
        var builder = PostgresqlExtensions.PostgresqlDatabase(manager, "prodpol");
        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".seed."),
            new SqlScriptOptions { RunGroupOrder = 100 });

        builder.WithScriptSorter(new DefaultScriptSorter());

        builder.LogTo(quietLog);
        builder.WithTransaction();
        builder.JournalTo(new ProdpolTableJournal(
            () => manager,
            () => quietLog,
            "prodpol_meta",
            "seed_migrations"
        ));

        return builder.Build();
    }
}