namespace Storczyk.Prodpol.Core.Services

open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Async
open Storczyk.Async.AsyncResult
open Storczyk.Async.Task
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<IEmployeeRoleReadRepository>)>]
type PgEmployeeRoleReadRepository(dataSource: NpgsqlDataSource) =
    interface IEmployeeRoleReadRepository with
        member this.GetAllAsync(token) =
            async {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! reader =
                    wrapTask (
                        conn.QueryAsync<EmployeeRoleRead>(
                            """SELECT er.role_id,
                                        er.display_name,
                                        er.role_name,
                                        COUNT(e.employee_id) AS employees_count
                                    FROM prodpol.employee_roles er
                                    LEFT JOIN prodpol.employees e ON e.role_id = er.role_id
                                    GROUP BY er.role_id, er.display_name, er.role_name
                                    ORDER BY er.role_id;"""
                        )
                    )

                return reader |> AsAsyncEnumerable
            }

        member this.CountAsync(token) =
            async {
                use! conn = dataSource.OpenConnectionAsync(token)
                return! wrapTask (conn.ExecuteScalarAsync<int64>("SELECT COUNT(*) FROM prodpol.employee_roles;"))
            }

        member this.GetByIdAsync(key) =
            async {
                let! ct = Async.CancellationToken
                let! conn = dataSource.OpenConnectionAsync(ct)

                let! role =
                    wrapOpt (fun () ->
                        conn.QuerySingleOrDefaultAsync<EmployeeRoleRead option>(
                            """SELECT er.role_id,
                                        er.display_name,
                                        er.role_name,
                                        COUNT(e.employee_id) AS employees_count
                                    FROM prodpol.employee_roles er
                                    LEFT JOIN prodpol.employees e ON e.role_id = er.role_id
                                    WHERE er.role_name = @id
                                    GROUP BY er.role_id, er.display_name, er.role_name;""",
                            param = {| id = key |},
                            commandTimeout = 1000
                        ))

                return role
            }
