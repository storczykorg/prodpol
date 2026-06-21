namespace Storczyk.Prodpol.Core.Services

open System
open System.Linq
open Dapper
open Npgsql
open Storczyk.Async
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Models.UtilHelpers
open Storczyk.Prodpol.Core.Utils

[<RegisterAsTransient(typeof<IEmployeeSearchRepository>)>]
type PgEmployeeSearchRepository(dataSource: NpgsqlDataSource) =
    interface IEmployeeSearchRepository with
        member this.CountSearchAsync(options: EmployeeSearchOption, token) =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)

                let sql = """SELECT count(*) FROM prodpol.filtered_employees(
                                _fullname := (@fullName)::varchar,
                                _email := (@email)::varchar,
                                _phone_number := (@phoneNumber)::varchar,
                                _role_names := (@roleNames)::varchar array
                                );
                            """
                let param = {|
                                email = options.email |> Option.defaultValue "";
                                phoneNumber = options.phoneNumber |> Option.defaultValue "";
                                fullname = options.fullName |> Option.defaultValue "";
                                roleNames = options.roleNames |> Option.defaultValue [||] |}

                return!
                    wrapTask
                        (conn.QuerySingleAsync<int>(
                             sql,
                            param = param
                            )
                        )
            }

        member this.SearchAsync(options: EmployeeSearchOption, token): AsyncResult<EmployeeSearchResult> =
            asyncResult {
                use! conn = dataSource.OpenConnectionAsync(token)

                let sql =
                    """SELECT * FROM prodpol.ordered_employees(
                                _asc := (@asc)::boolean,
                                _fullname := (@fullName)::varchar,
                                _email := (@email)::varchar,
                                _phone_number := (@phoneNumber)::varchar,
                                _ordering_key := (@orderBy)::prodpol.employee_ordering_keys,
                                _role_names := (@roleNames)::varchar array
                                )
                        offset (@skip)::integer
                        limit (@limit)::integer;
                            """

                let! _emps: EmployeeRead seq = conn.QueryAsync<EmployeeRead>(
                                         sql,
                                         param = {|
                                                orderBy =
                                                    ((options.orderBy |> Option.defaultValue EmployeeOrderKeys.EmployeeId)
                                                     |> GetSqlName)
                                                limit = options.limit
                                                skip = options.skip
                                                asc = options.asc
                                                email = options.email |> Option.defaultValue ""
                                                phoneNumber = options.phoneNumber |> Option.defaultValue ""
                                                fullname = options.fullName |> Option.defaultValue ""
                                                roleNames = options.roleNames |> Option.defaultValue [||]
                                            |}
                                        )
                let emps = _emps.ToArray()

                let! _count = (this :> IEmployeeSearchRepository).CountSearchAsync(options, token)
                let count = int64(_count)

                let nextCursor = if emps.Length > 0 then Some (emps.LongLength + int64(options.skip)) else None

                let result = EmployeeSearchResult(results = emps,
                                            nextCursor = nextCursor,
                                            total = count)
                return result
            }
