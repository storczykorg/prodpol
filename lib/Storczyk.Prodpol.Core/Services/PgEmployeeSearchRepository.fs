namespace Storczyk.Prodpol.Core.Services

open System
open System.Linq
open Dapper
open FSharp.Control
open Npgsql
open Storczyk.Prodpol.Core.Data
open Storczyk.Prodpol.Core.Models
open Storczyk.Prodpol.Core.Models.UtilHelpers
open Storczyk.Prodpol.Core.Utils
open Storczyk.Prodpol.Core.Utils.AsyncResult
open Storczyk.Prodpol.Core.Utils.Task

type PgEmployeeSearchRepository(dataSource: NpgsqlDataSource) =
    interface IEmployeeSearchRepository with
        member this.CountSearchAsync(options: EmployeeSearchOption, token) =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)
                
                return!
                    wrap (fun () ->
                        conn.QuerySingleAsync<int>(
                            """SELECT count(*) FROM prodpol.ordered_employees(
                                _ordering_key := (@orderBy)::prodpol.employee_ordering_keys,
                                _asc := (@asc),
                                _fullname := (@fullName),
                                _email := (@email),
                                _phone_number := (@phoneNumber)
                                );
                            """,
                            param = {|
                                      orderBy = GetSqlName EmployeeOrderKeys.EmployeeId
                                      asc = options.asc
                                      email = options.email |> Option.defaultValue ""
                                      phoneNumber = options.phoneNumber |> Option.defaultValue ""
                                      fullname = options.fullName |> Option.defaultValue ""
                                      |}
                        ))
            }
        
        member this.SearchAsync(options: EmployeeSearchOption, token) =
            asyncResult {
                let! conn = dataSource.OpenConnectionAsync(token)

                let! _emps =
                    wrap (fun () ->
                        conn.QueryAsync<EmployeeRead>(
                            """SELECT * FROM prodpol.ordered_employees(
                                _ordering_key := (@orderBy)::prodpol.employee_ordering_keys,
                                _asc := (@asc),
                                _previous_employee := (@cursor),
                                _limit := (@limit),
                                _fullname := (@fullName),
                                _email := (@email),
                                _phone_number := (@phoneNumber)
                                );
                            """,
                            param = {|
                                      orderBy = ((options.orderBy
                                                  |> Option.defaultValue EmployeeOrderKeys.EmployeeId)
                                                  |> GetSqlName)
                                      limit = options.limit
                                      asc = options.asc
                                      cursor = options.cursor |> Option.defaultValue -1
                                      email = options.email |> Option.defaultValue ""
                                      phoneNumber = options.phoneNumber |> Option.defaultValue ""
                                      fullname = options.fullName |> Option.defaultValue ""
                                      |}
                        ))

                let emps = _emps.ToArray()

                let _cursor = if emps.Length > 0 then Some(emps.Last().Id) else None
                
                let! count = (this :> IEmployeeSearchRepository).CountSearchAsync(options, token)

                return EmployeeSearchResult(results = emps, nextCursor = _cursor, total = count)
            }
