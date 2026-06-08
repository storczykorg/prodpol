using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Sorters;
using DbUp.Postgresql;
using DbUp.Support;

namespace Storczyk.Database.Services;

public class PostgresUpgrader(
    PostgresqlConnectionManager manager,
    ILogger<PostgresUpgrader> logger
)
{
    public virtual UpgradeEngine Build()
    {
        var builder = PostgresqlExtensions.PostgresqlDatabase(manager, "prodpol");
        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".schemas."),
            new SqlScriptOptions { RunGroupOrder = 1, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".types."),
            new SqlScriptOptions { RunGroupOrder = 3, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".utils."),
            new SqlScriptOptions { RunGroupOrder = 5, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".dictionaries."),
            new SqlScriptOptions { RunGroupOrder = 10, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".tables."),
            new SqlScriptOptions { RunGroupOrder = 13, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".checks."),
            new SqlScriptOptions { RunGroupOrder = 15, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".foreign_keys."),
            new SqlScriptOptions { RunGroupOrder = 20, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".indices."),
            new SqlScriptOptions { RunGroupOrder = 25, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".views."),
            new SqlScriptOptions { RunGroupOrder = 30, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".procedures."),
            new SqlScriptOptions { RunGroupOrder = 35, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".reports."),
            new SqlScriptOptions { RunGroupOrder = 40, ScriptType = ScriptType.RunAlways });
        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".triggers."),
            new SqlScriptOptions { RunGroupOrder = 45, ScriptType = ScriptType.RunAlways });

        builder.WithScriptSorter(new DefaultScriptSorter());

        builder.LogTo(logger);
        builder.WithTransaction();

        builder.JournalTo(new ProdpolTableJournal(
            () => manager,
            () => new MicrosoftUpgradeLog(logger),
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
        var builder = PostgresqlExtensions.PostgresqlDatabase(manager, "prodpol");
        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains(".seed."),
            new SqlScriptOptions { RunGroupOrder = 100, ScriptType = ScriptType.RunAlways });

        builder.WithScriptSorter(new DefaultScriptSorter());

        builder.LogTo(logger);
        builder.WithTransaction();

        return builder.Build();
    }
}