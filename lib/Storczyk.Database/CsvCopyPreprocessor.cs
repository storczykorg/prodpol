using DbUp.Engine;

namespace Storczyk.Database;

public class CsvCopyPreprocessor : IScriptPreprocessor
{
    protected readonly string TableName;
    protected readonly string SchemaName;

    public CsvCopyPreprocessor(string tableName, string schemaName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentException("Table name cannot be null or empty");
        if (string.IsNullOrEmpty(schemaName))
            throw new ArgumentException("Schema name cannot be null or empty");

        TableName = tableName;
        SchemaName = schemaName;
    }

    public string Process(string contents)
    {
        return string.IsNullOrEmpty(contents!.Trim())
            ? throw new ArgumentException($"{nameof(contents)} is null or empty.)")
            : $"""
               DO $$
               BEGIN
                   -- Ensure your table exists and matches these headers exactly
                   COPY "{SchemaName}"."{TableName}"
                   FROM STDIN WITH (FORMAT CSV, HEADER);
               END
               $$;
               {contents}
               """;
    }
}