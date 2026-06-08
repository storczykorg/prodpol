using DbUp.Engine;
using DbUp.Engine.Sorters;
using DbUp.Postgresql;
using DbUp.Support;

namespace Storczyk.Database.Services;

public class PostgresNuke(
    PostgresqlConnectionManager manager,
    ILogger<PostgresNuke> logger
)
{
    public virtual UpgradeEngine Build()
    {
        var builder = PostgresqlExtensions.PostgresqlDatabase(manager, "prodpol");
        builder.WithScriptsEmbeddedInAssembly(typeof(PostgresUpgraderExtensions).Assembly,
            path => path.Contains("nuclear"),
            new SqlScriptOptions { RunGroupOrder = 0, ScriptType = ScriptType.RunAlways });

        builder.WithScriptSorter(new DefaultScriptSorter());

        builder.LogTo(logger);

        return builder.Build();
    }
}