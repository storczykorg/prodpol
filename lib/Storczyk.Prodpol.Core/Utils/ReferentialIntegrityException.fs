namespace Storczyk.Prodpol.Core.Utils

type ReferentialIntegrityException(table: string, column: string, inner: System.Exception) =
    inherit System.Exception($"Cannot delete record because \"{table}.{column}\" still references it.", inner)
    member this.TableName = table
    member this.ColumnName = column
