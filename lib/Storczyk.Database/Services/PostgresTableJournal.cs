using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Postgresql;
using DbUp.Support;

namespace Storczyk.Database.Services;

internal class ProdpolTableJournal : TableJournal
{
    internal ProdpolTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema,
        string tableName)
        : base(connectionManager, logger, new PostgresqlObjectParser(), schema, tableName)
    {
    }


    protected override string GetInsertJournalEntrySql(string scriptName, string applied)
    {
        return $"""
                insert into {FqSchemaTableName} (scriptname, applied) 
                values ({scriptName}, {applied})
                """;
    }

    protected override string GetJournalEntriesSql()
    {
        return $"select ScriptName from {FqSchemaTableName} order by ScriptName";
    }

    protected override string CreateSchemaTableSql(string quotedPrimaryKeyName)
    {
        return
            $"""
             CREATE SCHEMA IF NOT EXISTS {SchemaTableSchema}; CREATE TABLE {FqSchemaTableName}
             (
                 schemaversionsid serial NOT NULL,
                 scriptname character varying(255) NOT NULL,
                 applied timestamp without time zone NOT NULL,
                 CONSTRAINT {quotedPrimaryKeyName} PRIMARY KEY (schemaversionsid)
             );
             """;
    }
}