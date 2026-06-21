namespace Storczyk.Prodpol.Core.Services

open System
open System.Threading
open Dapper
open Npgsql
open Storczyk.Async
open Storczyk.Async.AsyncResult
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<IRepository<int64, EmployeePhoto>>)>]
type PgEmployeePhotoRepository(dataSource: NpgsqlDataSource) =
    interface IRepository<int64, EmployeePhoto> with
        member this.AddAsync(entity) =
            asyncResult {
                use! conn = dataSource.OpenConnectionAsync()
                use! scope = conn.BeginTransactionAsync()

                let sql = """
                INSERT INTO prodpol.employee_photos(employee_id, mime_type, payload, payload_size)
                values ((@id)::bigint, (@mime)::varchar(128), (@payload)::bytea, (@payload_size)::integer);
                """

                let param = {| id = entity.EmployeeId; mime = entity.MimeType; payload = entity.Payload; payload_size = entity.Payload.Length |}

                let! results = wrapTask(conn.ExecuteAsync(
                    sql, param))

                if results = 1 then
                    do! scope.CommitAsync()
            }
        member this.CountAsync(token) =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)
                let cnt = wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employee_photos;"))
                return! cnt
            }
        member this.DeleteAsync(key) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "DELETE FROM prodpol.employee_photos
                        WHERE employee_id = @id;",
                        {| id = key |}
                    )
                with
                | 0 ->
                    do! scope.RollbackAsync()
                    return Error DatabaseError.NotFound
                | 1 ->
                    do! scope.CommitAsync()
                    return Ok()
                | _ ->
                    do! scope.RollbackAsync()

                    return
                        Error(
                            DatabaseError.UnknownException(
                                InvalidOperationException
                                    "Expected affected rows to be a value between 0 and 1. Evaluate query"
                            )
                        )
            }
        member this.GetAllAsync(token) =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (conn.QueryAsync<EmployeePhoto>(
                            "SELECT * FROM prodpol.employee_photos
                            ORDER BY employee_id;"
                        ))
                return reader |> AsAsyncEnumerable
            }
        member this.GetByIdAsync(key) =
            asyncResult {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                let! emp : EmployeePhoto | null =
                    wrapTask (
                        conn.QuerySingleOrDefaultAsync<EmployeePhoto | null>(
                            "SELECT * FROM prodpol.employee_photos WHERE employee_id = @id;",
                            param = {| id = key |},
                            commandTimeout = 1000
                        ))

                match emp with
                | null -> return! Error DatabaseError.NotFound
                | _ -> return emp
            }
        member this.UpdateAsync key entity =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                use! scope = conn.BeginTransactionAsync(ct)

                match!
                    conn.ExecuteAsync(
                        // language=postgresql
                        "UPDATE prodpol.employee_photos
                        SET
                            employee_id = @newKey,
                            payload = @payload,
                            mime_type = @mime_type,
                            payload_size = @payload_size
                        WHERE employee_id = @oldId;",
                        {| oldId = key
                           newKey = entity.EmployeeId
                           payload = entity.Payload
                           mime_type = entity.MimeType
                           payload_size = entity.PayloadSize
                           |}
                    )
                with
                | 0 ->
                    do! scope.RollbackAsync()
                    return Error DatabaseError.NotFound
                | 1 ->
                    do! scope.CommitAsync()
                    return Ok()
                | _ ->
                    do! scope.RollbackAsync()

                    return
                        Error(
                            DatabaseError.UnknownException(
                                InvalidOperationException
                                    "Expected affected rows to be a value between 0 and 1. Evaluate query"
                            )
                        )

            }
