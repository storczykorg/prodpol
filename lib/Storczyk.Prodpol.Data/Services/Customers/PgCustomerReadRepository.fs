namespace Storczyk.Prodpol.Core.Services

open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<ICustomerReadRepository>)>]
type PgCustomerReadRepository(dataSource: NpgsqlDataSource) =
    interface ICustomerReadRepository with
        member this.GetAllAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (
                        conn.QueryAsync<CustomerRead>(
                            "SELECT * FROM prodpol.customers_with_roles ORDER BY customer_id;"
                        )
                    )

                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.customers;"))
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)

                let! customer =
                    wrapTask (
                        conn.QuerySingleOrDefaultAsync<CustomerRead>(
                            "SELECT * FROM prodpol.customers_with_roles WHERE customer_id = @id;",
                            param = {| id = key |},
                            commandTimeout = 1000
                        )
                    )

                return customer
            }
