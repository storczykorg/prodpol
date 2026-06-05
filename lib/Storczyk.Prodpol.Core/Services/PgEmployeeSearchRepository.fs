namespace Storczyk.Prodpol.Core.Services

open System.Linq
open Dapper
open Npgsql
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Models.UtilHelpers
open Storczyk.Prodpol.Core.Utils

type PgEmployeeSearchRepository(dataSource: NpgsqlDataSource) =
    interface IEmployeeSearchRepository with
        member this.CountSearchAsync(options: EmployeeSearchOption, token) =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)

                return!
                    wrapTask
                        (conn.QuerySingleAsync<int>(
                             """SELECT count(*) FROM prodpol.filtered_employees(
                                _fullname := (@fullName)::varchar,
                                _email := (@email)::varchar,
                                _phone_number := (@phoneNumber)::varchar,
                                _role_names := (@roleNames)::varchar array,
                                _cursor_id := NULL
                                );
                            """,
                            param =
                             {| orderBy = GetSqlName EmployeeOrderKeys.EmployeeId
                                asc = options.asc
                                email = options.email |> Option.defaultValue ""
                                phoneNumber = options.phoneNumber |> Option.defaultValue ""
                                fullname = options.fullName |> Option.defaultValue ""
                                roleNames = options.roleNames |> Option.defaultValue [||] |}
                            )
                        )
            }

        member this.SearchAsync(options: EmployeeSearchOption, token): AsyncResult<EmployeeSearchResult> =
            asyncResult {
                use! conn = dataSource.OpenConnectionAsync(token)

                let sql =
                    """SELECT * FROM prodpol.ordered_employees(
                                _ordering_key := (@orderBy)::prodpol.employee_ordering_keys,
                                _asc := (@asc)::boolean,
                                _cursor_id := (@cursor)::bigint,
                                _limit := (@limit)::int,
                                _fullname := (@fullName)::varchar,
                                _email := (@email)::varchar,
                                _phone_number := (@phoneNumber)::varchar,
                                _role_names := (@roleNames)::varchar array
                                );
                            """

                let! _emps: EmployeeRead seq = conn.QueryAsync<EmployeeRead>(
                                         sql,
                                         param = {|
                                                orderBy =
                                                    ((options.orderBy |> Option.defaultValue EmployeeOrderKeys.EmployeeId)
                                                     |> GetSqlName)
                                                limit = options.limit
                                                asc = options.asc
                                                cursor = options.cursor |> Option.defaultValue -1
                                                email = options.email |> Option.defaultValue ""
                                                phoneNumber = options.phoneNumber |> Option.defaultValue ""
                                                fullname = options.fullName |> Option.defaultValue ""
                                                roleNames = options.roleNames |> Option.defaultValue [||]
                                            |}
                                        )
                let emps = _emps.ToArray()

                let! _count = (this :> IEmployeeSearchRepository).CountSearchAsync(options, token)
                let count = int64(_count)

                let result = EmployeeSearchResult(results = emps,
                                            nextCursor = options.cursor,
                                            total = count)
                return result
            }
