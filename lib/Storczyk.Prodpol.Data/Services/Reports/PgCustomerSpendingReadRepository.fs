namespace Storczyk.Prodpol.Core.Services

open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Async
open Storczyk.Async.AsyncResult
open Storczyk.Async.Task
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Data.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<ICustomerSpendingReadRepository>)>]
type PgCustomerSpendingReadRepository(dataSource: NpgsqlDataSource) =
    interface ICustomerSpendingReadRepository with
        member this.GetAllAsync(token) =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (
                        conn.QueryAsync<CustomerSpending>(
                            """SELECT customer_id,
                                        full_name,
                                        email,
                                        total_orders,
                                        total_spent,
                                        average_order_value,
                                        p25,
                                        p50,
                                        p75,
                                        most_expensive_order,
                                        last_order_date
                                    FROM prodpol.customer_spending_report ORDER BY customer_id ASC;"""
                        )
                    )

                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            asyncResult {
                use! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>(
                    "SELECT COUNT(*) FROM prodpol.customer_spending_report;"
                ))
            }

        member this.GetByIdAsync(key: int64) =
            asyncResult {
                let! ct = Async.CancellationToken
                use! conn = dataSource.OpenConnectionAsync(ct)

                let! spending: CustomerSpending =
                    wrapOpt (fun () ->
                        conn.QuerySingleOrDefaultAsync<CustomerSpending option>(
                            """SELECT customer_id,
                                        full_name,
                                        email,
                                        total_orders,
                                        total_spent,
                                        average_order_value,
                                        p25,
                                        p50,
                                        p75,
                                        most_expensive_order,
                                        last_order_date
                                    FROM prodpol.customer_spending_report
                                    WHERE customer_id = @id;"""
                            , param = {| id = key |}
                        )
                    )

                return spending
            }
