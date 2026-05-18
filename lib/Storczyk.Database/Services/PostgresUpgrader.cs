using System.Text;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Sorters;
using DbUp.Engine.Transactions;
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
            path => path.Contains("schemas"),
            new SqlScriptOptions { RunGroupOrder = 5, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("utils"),
            new SqlScriptOptions { RunGroupOrder = 5, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("tables"),
            new SqlScriptOptions { RunGroupOrder = 10, ScriptType = ScriptType.RunAlways });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("checks"),
            new SqlScriptOptions { RunGroupOrder = 20, ScriptType = ScriptType.RunOnce });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("foreign_keys"),
            new SqlScriptOptions { RunGroupOrder = 30, ScriptType = ScriptType.RunOnce });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("indices"),
            new SqlScriptOptions { RunGroupOrder = 10, ScriptType = ScriptType.RunOnce });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("views"),
            new SqlScriptOptions { RunGroupOrder = 50, ScriptType = ScriptType.RunOnce });

        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("procedures"),
            new SqlScriptOptions { RunGroupOrder = 60, ScriptType = ScriptType.RunOnce });

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